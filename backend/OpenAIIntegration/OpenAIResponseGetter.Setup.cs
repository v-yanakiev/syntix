using OpenAI.Chat;

namespace OpenAIIntegration;

public partial class OpenAIResponseGetter
{
    private ChatClient CreateOpenAIClient(string languageModel)
    {
        return OpenAIConfiguration.GetChatClient(configuration["OpenAI_API_key"]!, languageModel);
    }

    private ChatCompletionOptions CreateCodeExecutionOptions()
    {
        return new ChatCompletionOptions { Tools = { OpenAIConfiguration.FunctionTool } };
    }
}