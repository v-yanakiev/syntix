using System.Text;
using System.Text.Json;
using System.Web;
using AbstractExecution;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Models;

namespace CommonAI;

public static class CommonAITooling
{
    public static async Task AdjustUserBalance(UserInfo user, decimal newBalance,
        IDbContextFactory<PostgresContext> contextFactory)
    {
        await using var scopedContext = await contextFactory.CreateDbContextAsync();
        scopedContext.Attach(user);
        user.Balance = newBalance;
        await scopedContext.SaveChangesAsync();
    }
    public static async Task LogExecution(UserInfo user, CodeExecutionParameters codeExecutionParameters,
        IDbContextFactory<PostgresContext> contextFactory)
    {
        var serializedCodeExecutionParameters = JsonSerializer.Serialize(codeExecutionParameters);
        
        await using var scopedContext = await contextFactory.CreateDbContextAsync();
        var logContent =
            $"User with email {user.Email} and Id {user.Id}, at time {DateTime.UtcNow}, requested a code execution with the following parameters: {serializedCodeExecutionParameters}";
        
        var log = new Log()
        {
            Content = logContent
        };
        scopedContext.Logs.Add(log);
        
        await scopedContext.SaveChangesAsync();
    }

    public static async Task SendMessageToUserChat(HttpContext httpContext, string contentString,
        StringBuilder? relevantContentBuilder = null)
    {
        relevantContentBuilder?.Append(contentString);

        var eventData = $"data: {{\"text\": \"{HttpUtility.JavaScriptStringEncode(contentString)}\"}}\n\n";

        await httpContext.Response.WriteAsync(eventData);
        await httpContext.Response.Body.FlushAsync();
    }

    public static async Task KeepAliveAsync(HttpContext httpContext, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(1000, cancellationToken); // Send keepalive every 1 second
            if (!cancellationToken.IsCancellationRequested) await SendMessageToUserChat(httpContext, "");
        }
    }
}