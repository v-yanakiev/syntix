using AbstractExecution;
using aiExecBackend.Extensions;
using DTOs.MessageDTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Enums;
using OpenAIIntegration;

namespace aiExecBackend.Endpoints;

public static class MessageEndpoints
{
    public static async Task GetCompletion(GetCompletionDTO getCompletionInfo, OpenAIResponseGetter openAIResponseGetter,
        SignInManager<UserInfo> signInManager,
        PostgresContext postgresContext, CodeExecutor codeExecutor, HttpContext httpContext,
        IConfiguration configuration, IDbContextFactory<PostgresContext> contextFactory)
    {
        httpContext.Response.ContentType = "text/event-stream";

        var user = await signInManager.GetUserWithExecutedExpression(a => a.Include(b => b.Chats));
        if (user == null) return;

        var template = await postgresContext.FlyMachineTemplates.FirstOrDefaultAsync(a =>
            a.Id == getCompletionInfo.SelectedTemplateId && (a.CreatorId == null || a.CreatorId == user.Id));
        if (template == null) return;
        
        var messages = getCompletionInfo.Messages;
        messages[^1] = messages[^1] with
        {
            Text = $"""
                    Execution Environment: {(Enum.IsDefined(typeof(CodeExecutionEnvironment), template.Name) ? template.Name : "Custom")}\n
                    {(template.ProgrammingLanguage != null ? $"Programming language to generate: {template.ProgrammingLanguage}\n" : "")}
                    {messages[^1].Text}
                    """
        };


        var requestingMessage = new Message()
        {
            ChatId = getCompletionInfo.ChatId, Content = messages[^1].Text, Role = "user"
        };
        var chat = user.Chats.FirstOrDefault(a => a.Id == getCompletionInfo.ChatId);
        if (chat == null) return;

        chat.Messages.Add(requestingMessage);

        
        await openAIResponseGetter.GetResponse(getCompletionInfo, user, chat, httpContext);

        await postgresContext.SaveChangesAsync();
    }

    public static async Task<IResult> GetAllMessagesInChatHandler(Guid chatId, SignInManager<UserInfo> signInManager)
    {
        var user = await signInManager.GetUserWithExecutedExpression(a =>
            a.Include(b => b.Chats).ThenInclude(b => b.Messages));
        if (user == null) return Results.NotFound("User not found!");

        var messages = user.Chats.First(a => a.Id == chatId)
            .Messages.Where(a => a.Role != "tool_call")
            .OrderBy(a => a.CreatedAt)
            .Select(MessageInfoDTO.FromMessage)
            .ToList();
        return Results.Ok(messages);
    }
}