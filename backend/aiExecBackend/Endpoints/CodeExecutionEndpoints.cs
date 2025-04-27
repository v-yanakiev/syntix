using AbstractExecution;
using aiExecBackend.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Models;
using OpenAI.Chat;
using OpenAIIntegration;

namespace aiExecBackend.Endpoints;

public static class CodeExecutionEndpoints
{
    public static async Task GetExecutionResult(CodeExecutionRequest codeExecutionRequest,
        SignInManager<UserInfo> signInManager,
        PostgresContext postgresContext, IExecutionSetup executionSetup, OpenAIResponseGetter openAIResponseGetter,
        HttpContext httpContext,
        IConfiguration configuration, IDbContextFactory<PostgresContext> contextFactory)
    {
        httpContext.Response.ContentType = "text/event-stream";

        var user = await signInManager.GetUserWithExecutedExpression(a => a.Include(b => b.Chats));
        if (user == null) return;

        var chat = user.Chats.FirstOrDefault(a => a.Id == codeExecutionRequest.ChatId);
        if (chat == null)
        {
            var otherUserChatWithSameId =
                postgresContext.Chats.FirstOrDefault(a => a.Id == codeExecutionRequest.ChatId);
            if (otherUserChatWithSameId != null) throw new Exception("Invalid chat id!");

            chat = new Chat
            {
                Id = codeExecutionRequest.ChatId, CreatorId = user.Id, Name = codeExecutionRequest.ChatId.ToString()
            };
            postgresContext.Chats.Add(chat);
            await postgresContext.SaveChangesAsync();
        }


        try
        {
            var messageToPassToPreprocessingAIContent =
                $"""
                    You are a preprocessing and code execution AI for a code execution and validation system.
                    When you are given a piece of code to execute and validate, if it references code, which is not defined in the piece of code you've been given,
                    you should prepend/append the missing pieces of code. These missing pieces of code might be dependency imports, or other user code. 
                    In case it's a dependency, install it and import it.
                    In case it's other user code, make a best-guess for its behavior (the one MOST LIKELY to lead to the validated code being correct), and 
                    prepend it to the code to be validated. 
                    DO NOT ASK THE USER ANYTHING, AS HE WILL NOT BE ABLE TO RESPOND TO YOU.
                    ALWAYS send the user the code you'll be executing.
                    NEVER change the original code, as that would defeat the purpose of validating it.
                    Try to make the code work at most 5 times - terminate afterward.
                    According to the preceding rules, execute the following code using function calling. 
                     {(string.IsNullOrWhiteSpace(codeExecutionRequest.Language) ? "" : $"Language: {codeExecutionRequest.Language}\n")}
                     Code: {codeExecutionRequest.Code}
                 """;

            var messageToPassToExecutionAI = new SystemChatMessage(messageToPassToPreprocessingAIContent);
            await openAIResponseGetter.GetResponse("gpt-4.1-mini", user, chat, [messageToPassToExecutionAI],
                httpContext, startEnvironmentForTemplateId: true, generatedCodeValidator: generatedCode =>
                {
                    if (!generatedCode.ContainsWhenBothStringsHaveWhitespaceRemoved(codeExecutionRequest.Code.Trim()))
                        return "Message to the AI: The original code wasn't in your generated code! Try again...";

                    return null;
                });

            await postgresContext.SaveChangesAsync();
        }
        finally
        {
            await executionSetup.StopMachinesAssociatedWithChatAsync(chat.Id, CancellationToken.None);
        }
    }
}

public record CodeExecutionRequest(Guid ChatId, string Code, string Language);