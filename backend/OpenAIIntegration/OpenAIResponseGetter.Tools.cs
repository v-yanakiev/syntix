using System.Text;
using AbstractExecution;
using OpenAI.Chat;

namespace OpenAIIntegration;

public partial class OpenAIResponseGetter
{
    private void CollectToolCallData(StreamingChatCompletionUpdate update, ToolCallCollector collector)
    {
        foreach (var toolCallUpdate in update.ToolCallUpdates)
        {
            // Collect tool call ID
            if (!string.IsNullOrEmpty(toolCallUpdate.ToolCallId))
                collector.ToolCallIds[toolCallUpdate.Index] = toolCallUpdate.ToolCallId;

            // Collect function name
            if (!string.IsNullOrEmpty(toolCallUpdate.FunctionName))
                collector.FunctionNames[toolCallUpdate.Index] = toolCallUpdate.FunctionName;

            // Append function arguments
            if (toolCallUpdate.FunctionArgumentsUpdate == null ||
                toolCallUpdate.FunctionArgumentsUpdate.ToArray().Length == 0)
                continue;

            var argumentsBuilder = collector.ArgumentBuilders.TryGetValue(toolCallUpdate.Index, out var existing)
                ? existing
                : new StringBuilder();
            argumentsBuilder.Append(toolCallUpdate.FunctionArgumentsUpdate);
            collector.ArgumentBuilders[toolCallUpdate.Index] = argumentsBuilder;
        }
    }

    private void BuildToolCalls(ToolCallCollector collector, StreamingResult result)
    {
        foreach (var indexToIdPair in collector.ToolCallIds)
        {
            var index = indexToIdPair.Key;
            var id = indexToIdPair.Value;

            // Create the function tool call
            var functionToolCall = ChatToolCall.CreateFunctionToolCall(
                id,
                collector.FunctionNames[index],
                BinaryData.FromString(collector.ArgumentBuilders[index].ToString()));

            result.ToolCalls.Add(functionToolCall);

            // Parse code execution parameters
            var codeParams = CodeExecutionParameters.Parse(functionToolCall.FunctionArguments.ToString());
            result.CodeExecutionParameters.Add(codeParams);
        }
    }
}