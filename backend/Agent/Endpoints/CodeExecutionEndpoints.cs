using System.Text.Json;
using Agent.Models;
using Agent.CodeExecution;
using Agent.Models;
using JsonSerializerContext = Agent.Models.JsonSerializerContext;

namespace Agent.Endpoints;

public static class CodeExecutionEndpoints
{
    public static async Task Handler(HttpContext context)
    {
        ExecutionRequest requestData;

        try
        {
            requestData = (await JsonSerializer.DeserializeAsync(context.Request.Body,
                JsonSerializerContext.Default.ExecutionRequest))!;
        }
        catch (Exception e)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync($"Invalid ExecutionRequest JSON. Error: {e.Message}");
            return;
        }

        context.Response.Headers.Add("Content-Type", "text/event-stream");
        context.Response.Headers.Add("Cache-Control", "no-cache");
        context.Response.StatusCode = 200;

        await CodeExecutor.Execute(requestData, context.Response);
    }
}