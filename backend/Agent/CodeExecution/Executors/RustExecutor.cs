using Agent.Models;
using Agent.OS;

namespace Agent.CodeExecution.Executors;

public static class RustExecutor
{
    public static async Task Execute(ExecutionRequest request, Func<string, Task> sendSSEMessage)
    {
        if (!GlobalState.Initialized)
        {
            await InitializeRustEnvironment(sendSSEMessage);
            GlobalState.Initialized = true;
        }

        var rustFilePath = Path.Combine(Constants.Execution.Directory,"src", "main.rs");
        if (request.DoNotCreateNewFile != true)
        {
            await File.WriteAllTextAsync(rustFilePath, request.Code);
        }

        await InstallRustDependenciesIfAny(request.Dependencies, sendSSEMessage);
        await ExecuteRustCode(sendSSEMessage);
    }

    private static async Task InitializeRustEnvironment(Func<string, Task> sendSSEMessage)
    {
        await ProcessRunner.RunAsync("cargo", "init --bin");
    }

    private static async Task ExecuteRustCode(Func<string, Task> sendSSEMessage)
    {
        await sendSSEMessage("Result from the execution of the Rust code:\n");
        await ProcessRunner.RunAsync("cargo", "run", sendSSEMessage, timeoutMilliseconds:5000);
    }

    private static async Task InstallRustDependenciesIfAny(string[]? dependencies, Func<string, Task> sendSSEMessage)
    {
        if (dependencies?.Length > 0)
        {
            await Common.LogDependencies(dependencies, sendSSEMessage);
            await sendSSEMessage("Installing dependencies...\n");
            
            foreach (var dependency in dependencies)
            {
                await ProcessRunner.RunAsync("cargo", $"add {dependency}", sendSSEMessage);
            }
            await ProcessRunner.RunAsync("cargo", "build", sendSSEMessage);
        }
    }
}