using AbstractExecution;
using aiExecBackend.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Enums;

namespace aiExecBackend.Endpoints;

public static class ChatEndpoints
{
    public static async Task<IResult> CreateChatHandler(SignInManager<UserInfo> signInManager,
        PostgresContext postgresContext, string chatName)
    {
        var user = await signInManager.UserManager.GetUserAsync(signInManager.Context.User);
        if (user == null)
        {
            return Results.NotFound("User not found");
        }

        var chat = new Chat() { CreatorId = user.Id, Name = chatName };
        postgresContext.Add(chat);
        await postgresContext.SaveChangesAsync();
        return Results.Ok(chat.Id);
    }

    public static async Task<IResult> DeleteChatHandler(Guid chatId, SignInManager<UserInfo> signInManager,
        PostgresContext postgresContext, IExecutionSetup executionSetup)
    {
        var user = await signInManager.GetUserWithExecutedExpression(a => a.Include(b => b.Chats));
        if (user == null)
        {
            return Results.NotFound("User not found!");
        }

        var chatToDelete = user.Chats.FirstOrDefault(a => a.Id == chatId);
        if (chatToDelete == null)
        {
            return Results.NotFound("Chat not found for specific user!");
        }

        await executionSetup.StopMachinesAssociatedWithChatAsync(chatId, CancellationToken.None);

        postgresContext.Chats.Remove(chatToDelete);
        await postgresContext.SaveChangesAsync();
        return Results.Ok();
    }

    public static async Task<IResult> StartMachineForChatHandler(StartMachineConfig startMachineConfig, PostgresContext postgresContext,
        SignInManager<UserInfo> signInManager,
        IExecutionSetup executionSetup)
    {
        var user = await signInManager.GetUserWithExecutedExpression(a => a.Include(b => b.Chats));
        if (user == null)
        {
            return Results.NotFound("User not found!");
        }

        var chatToDelete = user.Chats.FirstOrDefault(a => a.Id == startMachineConfig.ChatId);
        if (chatToDelete == null)
        {
            return Results.NotFound("Chat not found for specific user!");
        }
        
        await executionSetup.InitializeMachineAsync(startMachineConfig.ExecutionEnvironmentTemplateId, startMachineConfig.ChatId, user.Id);
        return Results.Ok();
    }
    
    public static async Task<IResult> GetUserChatsHandler(SignInManager<UserInfo> signInManager)
    {
        var user = await signInManager.GetUserWithExecutedExpression(a => a.Include(b => b.Chats));
        if (user == null)
        {
            return Results.NotFound("User not found!");
        }
        var chatsToReturn = user.Chats.OrderBy(a=>a.CreatedAt).Select(NameChatDTO.FromChat);
        return Results.Ok(chatsToReturn);
    }
}

public record StartMachineConfig(Guid ChatId, long ExecutionEnvironmentTemplateId);
public class NameChatDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; }

    public static NameChatDTO FromChat(Chat chat)
    {
        return new NameChatDTO { Id = chat.Id, Name = chat.Name, };
    }
}