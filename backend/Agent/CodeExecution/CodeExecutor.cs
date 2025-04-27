using System.Text.Json;
using Agent.CodeExecution.Executors;
using Agent.Models;
using JsonSerializerContext = Agent.Models.JsonSerializerContext;

namespace Agent.CodeExecution;

public static class CodeExecutor
{
    public static async Task Execute(ExecutionRequest request, HttpResponse response)
    {
        async Task SendSSEMessage(string message)
        {
            var jsonString = JsonSerializer.Serialize(new ServerSentEvent(new ServerSentMessage(message)),
                JsonSerializerContext.Default.ServerSentEvent);
            await response.WriteAsync($"{jsonString}\n\n");
            await response.Body.FlushAsync();
        }

        try
        {
            switch (request.Environment)
            {
                case CodeExecutionEnvironment.NodeJS:
                    await NodeJSExecutor.Execute(request, SendSSEMessage);
                    break;
                case CodeExecutionEnvironment.NodeTS:
                    await NodeTSExecutor.Execute(request, SendSSEMessage);
                    break;
                case CodeExecutionEnvironment.CSharp:
                    await CSharpExecutor.Execute(request, SendSSEMessage);
                    break;
                case CodeExecutionEnvironment.Java:
                    await JavaExecutor.Execute(request, SendSSEMessage);
                    break;
                case CodeExecutionEnvironment.Python:
                    await PythonExecutor.Execute(request, SendSSEMessage);
                    break;
                case CodeExecutionEnvironment.Go:
                    await GoExecutor.Execute(request, SendSSEMessage);
                    break;
                case CodeExecutionEnvironment.Rust:
                    await RustExecutor.Execute(request, SendSSEMessage);
                    break;
                case CodeExecutionEnvironment.PostgreSQL:
                    await PostgreSQLExecutor.Execute(request, SendSSEMessage);
                    break;
                case CodeExecutionEnvironment.Custom:
                    await CustomExecutor.Execute(request, SendSSEMessage);
                    break;
                default:
                    throw new Exception( $"aiExec CODE EXECUTION PROBLEM: aiExec doesn't support {request.Environment} yet!");
            }
        }
        catch (Exception ex)
        {
            await SendSSEMessage($"Error: {ex.Message}");
        }
    }
}