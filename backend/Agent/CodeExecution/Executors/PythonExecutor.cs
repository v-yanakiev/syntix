using Agent.Models;
using Agent.OS;

namespace Agent.CodeExecution.Executors;

public static class PythonExecutor
{
    public static async Task Execute(ExecutionRequest request, Func<string, Task> sendSSEMessage)
    {
        var pythonFilePath = Path.Combine(Constants.Execution.Directory, "script.py");
        if (request.DoNotCreateNewFile != true)
            await File.WriteAllTextAsync(pythonFilePath, request.Code);

        await InstallPythonDependenciesIfAny(request.Dependencies, sendSSEMessage);
        await ExecutePythonCode(sendSSEMessage);
    }

    private static async Task ExecutePythonCode(Func<string, Task> sendSSEMessage)
    {
        await sendSSEMessage("Result from the execution of the Python code:\n");
        await ProcessRunner.RunAsync("python3", "script.py", sendSSEMessage, timeoutMilliseconds:5000);
        await sendSSEMessage("\n");
    }

    private static async Task InstallPythonDependenciesIfAny(string[]? dependencies,
        Func<string, Task> sendSSEMessage)
    {
        if (dependencies?.Length > 0)
        {
            await Common.LogDependencies(dependencies, sendSSEMessage);
            await sendSSEMessage("Installing Python dependencies:\n");

            foreach (var dependency in dependencies)
            {
                var pipInstallCommand = $"pip install {dependency}";
                await sendSSEMessage($"Running: {pipInstallCommand}\n");
                await ProcessRunner.RunAsync("sh", $"-c \"{pipInstallCommand}\"", sendSSEMessage);
            }

            await sendSSEMessage("Dependency installation completed.\n");
        }
    }
}