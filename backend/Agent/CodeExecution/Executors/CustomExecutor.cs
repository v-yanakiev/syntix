using Agent.Models;
using Agent.OS;

namespace Agent.CodeExecution.Executors;

public static class CustomExecutor
{
    public static async Task Execute(ExecutionRequest request, Func<string, Task> sendSSEMessage)
    {
        if (!GlobalState.Initialized)
        {
            GlobalState.Initialized = true;
        }
        
        if (string.IsNullOrWhiteSpace(request.CodeFile))
        {
            throw new ArgumentException("CodeFile path cannot be null or empty.");
        }

        if (string.IsNullOrWhiteSpace(request.AfterChangesValidationCommand))
        {
            throw new ArgumentException("AfterChangesValidationCommand cannot be null or empty.");
        }
        
        if (string.IsNullOrWhiteSpace(request.CodeFile))
        {
            throw new ArgumentException("Invalid CodeFile path.");
        }

        if (request.DoNotCreateNewFile != true)
        {
            
            await File.WriteAllTextAsync(Path.Combine(Constants.Execution.Directory, request.CodeFile), request.Code);
        }
        
        await InstallCustomEnvironmentDependenciesIfAny(request.DependencyInstallingTerminalCall, request.Dependencies, Constants.Execution.Directory, sendSSEMessage);
        await ExecuteCustomEnvironmentCode(request.AfterChangesValidationCommand, Constants.Execution.Directory, sendSSEMessage);
    }
    
    private static async Task ExecuteCustomEnvironmentCode(string afterChangesValidationCommand, string workingDirectory, Func<string, Task> sendSSEMessage)
    {
        await sendSSEMessage("Validating changes...\n");
        await ProcessRunner.RunAsync("/bin/sh", $"-c \"{afterChangesValidationCommand}\"", sendSSEMessage, workingDirectory, timeoutMilliseconds:5000);

        await sendSSEMessage("\n");
    }

    static async Task InstallCustomEnvironmentDependenciesIfAny(string? dependencyInstallingTerminalCall, string[]? dependencies, string workingDirectory, Func<string, Task> sendSSEMessage)
    {
        if (dependencies?.Length > 0)
        {
            if (string.IsNullOrWhiteSpace(dependencyInstallingTerminalCall))
            {
                throw new Exception("You haven't configured a dependency installing terminal call for this environment, " +
                                    "yet are attempting to install dependencies!\n");
            }
            
            if (!dependencyInstallingTerminalCall.Contains("{0}"))
            {
                throw new ArgumentException("The dependency installing terminal call does not contain {0}.");
            }
            
            foreach(var dependency in dependencies)
            {
                if (string.IsNullOrWhiteSpace(dependency))
                {
                    throw new ArgumentException("Dependency name cannot be null or empty.");
                }
                
                var interpolatedDependencyInstallingTerminalCall = string.Format(dependencyInstallingTerminalCall, dependency);
                
                await sendSSEMessage($"Installing dependency {dependency}...\n");
                await ProcessRunner.RunAsync("/bin/sh", $"-c \"{interpolatedDependencyInstallingTerminalCall}\"", sendSSEMessage, workingDirectory);
            }
        }
    }
}
