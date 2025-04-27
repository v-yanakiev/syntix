using System.Text;
using AbstractExecution;
using CommonAI;
using DTOs.MessageDTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Models;
using OpenAI.Chat;

namespace OpenAIIntegration;

public partial class OpenAIResponseGetter(
    CodeExecutor codeExecutor,
    IExecutionSetup executionSetup,
    IConfiguration configuration,
    IDbContextFactory<PostgresContext> contextFactory,
    PostgresContext postgresContext)
{
    public async Task GetResponse(GetCompletionDTO getCompletionInfo,
        UserInfo user, Chat chat,
        HttpContext httpContext)
    {
        var messagesToComplete = getCompletionInfo.Messages.Select<CreateMessageDTO, ChatMessage>(a =>
                a.Role == "ai" ? new AssistantChatMessage(a.Text) : new UserChatMessage(a.Text))
            .ToList();

        await GetResponse(getCompletionInfo.LanguageModel, user, chat, messagesToComplete, httpContext,
            getCompletionInfo.SelectedTemplateId);
    }

    public async Task GetResponse(string languageModel, UserInfo user, Chat chat,
        List<ChatMessage> messagesToComplete,
        HttpContext httpContext,
        long? precalculatedTemplateId = null,
        bool? startEnvironmentForTemplateId = null,
        Func<string, string?>? generatedCodeValidator = null)
    {
        var openAIClient = CreateOpenAIClient(languageModel);
        var codeExecutionOptions = CreateCodeExecutionOptions();

        while (true)
        {
            if (!await CheckUserBalance(user, httpContext))
                break;

            var streamingResult = await ProcessOpenAIStream(
                openAIClient, messagesToComplete, codeExecutionOptions, httpContext, user, languageModel);

            await AddAssistantResponseToChat(chat, messagesToComplete, httpContext,
                streamingResult.ContentBuilder, streamingResult.Content);

            if (streamingResult.ToolCalls.Count == 0)
                break;

            await HandleToolCalls(
                chat, messagesToComplete, httpContext, streamingResult,
                user, precalculatedTemplateId, startEnvironmentForTemplateId, generatedCodeValidator);
        }
    }

    private async Task<StreamingResult> ProcessOpenAIStream(
        ChatClient client,
        List<ChatMessage> messagesToComplete,
        ChatCompletionOptions options,
        HttpContext httpContext,
        UserInfo user,
        string languageModel)
    {
        var updates = client.CompleteChatStreamingAsync(messagesToComplete, options);
        var result = new StreamingResult();

        var toolCallData = new ToolCallCollector();

        await foreach (var update in updates)
        {
            await ProcessContentUpdate(update, httpContext, result.ContentBuilder);
            CollectToolCallData(update, toolCallData);
            UpdateUserBalance(update, user, languageModel);
        }

        BuildToolCalls(toolCallData, result);
        SetResultContent(result);

        return result;
    }

    private async Task ProcessContentUpdate(
        StreamingChatCompletionUpdate update,
        HttpContext httpContext,
        StringBuilder contentBuilder)
    {
        foreach (var contentPart in update.ContentUpdate)
            await CommonAITooling.SendMessageToUserChat(httpContext, contentPart.Text, contentBuilder);
    }

    private void SetResultContent(StreamingResult result)
    {
        if (result.ContentBuilder.Length > 0)
            result.Content = result.ContentBuilder.ToString();
        else
            result.Content = string.Concat(result.CodeExecutionParameters.Select(p => p.GetUserVisibleSummary()));
    }

    private async Task AddAssistantResponseToChat(
        Chat chat,
        List<ChatMessage> messagesToComplete,
        HttpContext httpContext,
        StringBuilder contentBuilder,
        string content)
    {
        if (contentBuilder.Length == 0)
            await CommonAITooling.SendMessageToUserChat(httpContext, content, contentBuilder);

        // Add the assistant message to the chat
        chat.Messages.Add(new Message
        {
            ChatId = chat.Id,
            Content = content,
            Role = "ai"
        });

        messagesToComplete.Add(new AssistantChatMessage(content));
    }

    private async Task HandleToolCalls(
        Chat chat,
        List<ChatMessage> messagesToComplete,
        HttpContext httpContext,
        StreamingResult streamingResult,
        UserInfo user,
        long? precalculatedTemplateId,
        bool? startEnvironmentForTemplateId,
        Func<string, string?>? generatedCodeValidator)
    {
        for (var i = 0; i < streamingResult.ToolCalls.Count; i++)
        {
            var toolCall = streamingResult.ToolCalls[i];
            var codeParams = streamingResult.CodeExecutionParameters[i];

            // Validate code if validator provided
            if (generatedCodeValidator?.Invoke(codeParams.Code) is { } error)
            {
                messagesToComplete.Add(new SystemChatMessage(error));
                continue;
            }

            // Execute the code
            var executionResult = await ExecuteCode(
                codeParams,
                httpContext,
                user,
                chat.Id,
                precalculatedTemplateId,
                startEnvironmentForTemplateId);

            // Add results to messages
            AddExecutionResultToMessages(chat, messagesToComplete, toolCall, executionResult);
        }
    }

    private async Task<string> ExecuteCode(
        CodeExecutionParameters codeParams,
        HttpContext httpContext,
        UserInfo user,
        Guid chatId,
        long? precalculatedTemplateId,
        bool? startEnvironmentForTemplateId)
    {
        StringBuilder outputBuilder = new();
        await CommonAITooling.SendMessageToUserChat(httpContext, "\n```\n", outputBuilder);

        try
        {
            using var cts = new CancellationTokenSource();
            var keepaliveTask = CommonAITooling.KeepAliveAsync(httpContext, cts.Token);

            // Get the execution environment
            precalculatedTemplateId = await ResolveEnvironmentTemplateId(
                codeParams,
                precalculatedTemplateId,
                cts.Token);

            // Initialize the machine if needed
            if (startEnvironmentForTemplateId.GetValueOrDefault())
                await executionSetup.InitializeMachineAsync(
                    precalculatedTemplateId.Value, chatId, user.Id);

            // Log execution
            _ = CommonAITooling.LogExecution(user, codeParams, contextFactory);

            // Execute the code
            await foreach (var message in codeExecutor.ExecuteCodeAsync(
                                   codeParams,
                                   precalculatedTemplateId.Value,
                                   chatId, user.Id)
                               .WithCancellation(cts.Token))
                await CommonAITooling.SendMessageToUserChat(httpContext, message, outputBuilder);

            await cts.CancelAsync();
            await SafelyCancelKeepAliveTask(keepaliveTask);
        }
        catch (Exception ex)
        {
            await CommonAITooling.SendMessageToUserChat(httpContext,
                $"Failed to execute code, due to the following error: {ex.Message}",
                outputBuilder);
        }

        await CommonAITooling.SendMessageToUserChat(httpContext, "\n```\n", outputBuilder);
        return outputBuilder.ToString();
    }

    private async Task SafelyCancelKeepAliveTask(Task keepaliveTask)
    {
        try
        {
            await keepaliveTask;
        }
        catch (OperationCanceledException)
        {
            // Expected exception
        }
    }

    private async Task<long> ResolveEnvironmentTemplateId(
        CodeExecutionParameters codeParams,
        long? precalculatedTemplateId,
        CancellationToken cancellationToken)
    {
        if (precalculatedTemplateId != null)
            return precalculatedTemplateId.Value;

        var environmentType = codeParams.Environment.ToString();
        var template = await postgresContext.FlyMachineTemplates.FirstOrDefaultAsync(
            a => a.Type == environmentType,
            cancellationToken);

        if (template == null) throw new Exception($"Failed to find execution environment: {environmentType}");

        return template.Id;
    }

    private void AddExecutionResultToMessages(
        Chat chat,
        List<ChatMessage> messagesToComplete,
        ChatToolCall toolCall,
        string executionResult)
    {
        messagesToComplete.Add(new AssistantChatMessage([toolCall]));
        messagesToComplete.Add(new ToolChatMessage(toolCall.Id, executionResult));

        chat.Messages.Add(new Message
        {
            ChatId = chat.Id, Content = executionResult, Role = "ai"
        });
        chat.Messages.Add(new Message
        {
            ChatId = chat.Id, Content = executionResult, Role = "tool_call"
        });
    }
}