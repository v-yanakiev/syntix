using Agent.Models;
using Agent.OS;

namespace Agent.CodeExecution.Executors;

public static class NodeTSExecutor
{
    public static async Task Execute(ExecutionRequest request, Func<string, Task> sendSSEMessage)
    {
        if (!GlobalState.Initialized)
        {
            await NodeJSExecutor.InitializeJSEnvironment();
            await InitializeTSEnvironment();
            GlobalState.Initialized = true;
        }
        var tsFilePath = Path.Combine(Constants.Execution.Directory, "code.ts");
        Directory.CreateDirectory(Path.GetDirectoryName(tsFilePath)!);

        if (request.DoNotCreateNewFile!=true) await File.WriteAllTextAsync(tsFilePath, request.Code);

        await NodeJSExecutor.InstallJsDependenciesIfAny(request.Dependencies,sendSSEMessage);

        await sendSSEMessage("Result from the tsc compiler:\n");
        await ProcessRunner.RunAsync("npx", "tsc --project .",sendSSEMessage);
        await sendSSEMessage("\n");

        await NodeJSExecutor.ExecuteJsCode(sendSSEMessage);
    }
    
    private static async Task InitializeTSEnvironment()
    {
        var tsconfigPath = Path.Combine(Constants.Execution.Directory, "tsconfig.json");
        var tsconfig = @"{
            ""compilerOptions"": {
                ""module"": ""commonjs"",
                ""target"": ""es2020"",
                ""types"": [""node""]
            }
        }";
        await File.WriteAllTextAsync(tsconfigPath, tsconfig);
        await ProcessRunner.RunAsync("npm", "install --save-dev @types/node");
    }
}