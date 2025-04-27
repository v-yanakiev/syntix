using System.Text;
using AbstractExecution;
using OpenAI.Chat;

namespace OpenAIIntegration;

public partial class OpenAIResponseGetter
{
    private class StreamingResult
    {
        public StringBuilder ContentBuilder { get; } = new();
        public List<ChatToolCall> ToolCalls { get; } = [];
        public List<CodeExecutionParameters> CodeExecutionParameters { get; } = [];
        public string Content { get; set; } = string.Empty;
    }

    private class ToolCallCollector
    {
        public Dictionary<int, string> ToolCallIds { get; } = [];
        public Dictionary<int, string> FunctionNames { get; } = [];
        public Dictionary<int, StringBuilder> ArgumentBuilders { get; } = [];
    }
}