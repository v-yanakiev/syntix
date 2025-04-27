using Agent.Models;
using Agent.OS;

namespace Agent.CodeExecution.Executors;

public static class CSharpExecutor
{
    public static async Task Execute(ExecutionRequest request, Func<string, Task> sendSSEMessage)
    {
        if (!GlobalState.Initialized)
        {
            await CreateProjectFile();
            GlobalState.Initialized = true;
        }
        
        var jsFilePath = Path.Combine(Constants.Execution.Directory, "code.cs");
        if (request.DoNotCreateNewFile!=true) await File.WriteAllTextAsync(jsFilePath, request.Code);
        await InstallCSharpDependenciesIfAny(request.Dependencies, sendSSEMessage);
        await ExecuteCSharpCode(sendSSEMessage);
    }
    
    private static async Task ExecuteCSharpCode(Func<string, Task> sendSSEMessage)
    {
        await sendSSEMessage("Result from building the C# project:\n");
        await ProcessRunner.RunAsync("dotnet", "build code.csproj", sendSSEMessage);

        await sendSSEMessage("Result from the execution of the C#:\n");
        await ProcessRunner.RunAsync("dotnet", "run --project code.csproj", sendSSEMessage, timeoutMilliseconds:5000);

        await sendSSEMessage("\n");
    }

    static async Task InstallCSharpDependenciesIfAny(string[]? dependencies, Func<string, Task> sendSSEMessage)
    {
        if (dependencies?.Length > 0)
        {
            await Common.LogDependencies(dependencies, sendSSEMessage);
        
            await sendSSEMessage("Installing NuGet packages:\n");
            foreach (var package in dependencies)
            {
                await sendSSEMessage($"Installing package {package}...\n");
                await ProcessRunner.RunAsync("dotnet", $"add package {package.Trim()}", sendSSEMessage);
            }
        }
    }
    
    private static async Task CreateProjectFile()
    {
        var csProjPath = Path.Combine(Constants.Execution.Directory, "code.csproj");

        var projectContent = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>";
        
        await File.WriteAllTextAsync(csProjPath, projectContent);
    }
}