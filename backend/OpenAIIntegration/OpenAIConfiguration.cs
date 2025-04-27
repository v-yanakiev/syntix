using AbstractExecution;
using OpenAI.Chat;

namespace OpenAIIntegration;

public static class OpenAIConfiguration
{
    public static ChatClient GetChatClient(string apiKey, string model)
    {
        return new ChatClient(model, apiKey);
    }

    public static ChatTool FunctionTool =>
        ChatTool.CreateFunctionTool(ExecutionConstants.CodeExecutionFunctionName,
            ExecutionConstants.CodeExecutionFunctionDescription,
            BinaryData.FromString(ExecutionConstants.CodeExecutionFunctionParameters));
}