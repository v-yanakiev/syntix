using Agent.Models;
using Agent.OS;

namespace Agent.CodeExecution.Executors;

public static class GoExecutor
{
    public static async Task Execute(ExecutionRequest request, Func<string, Task> sendSSEMessage)
    {
        if (!GlobalState.Initialized)
        {
            await InitializeGoEnvironment();
            GlobalState.Initialized = true;
        }

        var goFilePath = Path.Combine(Constants.Execution.Directory, "main.go");
        if (request.DoNotCreateNewFile != true)
        {
            await File.WriteAllTextAsync(goFilePath, request.Code);
        }

        await InstallGoDependenciesIfAny(request.Dependencies, sendSSEMessage);
        await ExecuteGoCode(sendSSEMessage);
    }

    private static async Task InitializeGoEnvironment()
    {
        // await ProcessRunner.RunAsync("go", "version", sendSSEMessage);
        await ProcessRunner.RunAsync("go", "mod init codeexecution");
    }

    private static async Task ExecuteGoCode(Func<string, Task> sendSSEMessage)
    {
        await sendSSEMessage("Result from the execution of the Go code:\n");
        await ProcessRunner.RunAsync("go", "build -o main main.go", sendSSEMessage);
        await ProcessRunner.RunAsync("chmod", "+x main");
        await ProcessRunner.RunAsync("/bin/bash", "-c './main'", sendSSEMessage, timeoutMilliseconds:5000);
    }

    private static async Task InstallGoDependenciesIfAny(string[]? dependencies, Func<string, Task> sendSSEMessage)
    {
        if (dependencies?.Length > 0)
        {
            await Common.LogDependencies(dependencies, sendSSEMessage);
            await sendSSEMessage("Installing dependencies...\n");
            foreach (var dependency in dependencies)
            {
                await ProcessRunner.RunAsync("go", $"get {dependency}", sendSSEMessage);
            }
        }
    }
}