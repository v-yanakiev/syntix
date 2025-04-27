using Agent.Models;
using Agent.OS;

namespace Agent.CodeExecution.Executors;

public static class NodeJSExecutor
{
    public static async Task Execute(ExecutionRequest request, Func<string, Task> sendSSEMessage)
    {
        if (!GlobalState.Initialized)
        {
            await InitializeJSEnvironment();
            GlobalState.Initialized = true;
        }

        var jsFilePath = Path.Combine(Constants.Execution.Directory, "code.js");
        if (request.DoNotCreateNewFile != true) await File.WriteAllTextAsync(jsFilePath, request.Code);
        await InstallJsDependenciesIfAny(request.Dependencies, sendSSEMessage);
        await ExecuteJsCode(sendSSEMessage);
    }

    public static async Task InitializeJSEnvironment()
    {
        await ProcessRunner.RunAsync("npm", "init -y");
    }

    public static async Task ExecuteJsCode(Func<string, Task> sendSSEMessage)
    {
        await sendSSEMessage("Result from the execution of the js:\n");
        await ProcessRunner.RunAsync("node", "code.js", sendSSEMessage, timeoutMilliseconds:5000);
        await sendSSEMessage("\n");
    }

    public static async Task InstallJsDependenciesIfAny(string[]? dependencies, Func<string, Task> sendSSEMessage)
    {
        if (dependencies?.Length > 0)
        {
            // Log dependencies list
            await Common.LogDependencies(dependencies, sendSSEMessage);

            // Install all dependencies in one command
            await sendSSEMessage("Installing dependencies:\n");
            await ProcessRunner.RunAsync("npm", $"install {string.Join(" ", dependencies)}", sendSSEMessage);
        }
    }
}