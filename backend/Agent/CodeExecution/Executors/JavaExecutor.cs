using System.Diagnostics;
using System.Text.RegularExpressions;
using Agent.Models;
using Agent.OS;

namespace Agent.CodeExecution.Executors;

public static partial class JavaExecutor
{
    private static readonly string JBangPath =
        GlobalState.IsProduction ? "/root/.jbang/bin/jbang" : "/usr/local/sdkman/candidates/jbang/current/bin/jbang";
    public static async Task Execute(ExecutionRequest request, Func<string, Task> sendSSEMessage)
    {
        var regex = MyRegex();
        var match = regex.Match(request.Code);
    
        if (!match.Success)
        {
            throw new Exception("No public class found in the Java code");
        }

        var className = match.Groups[1].Value;
        var javaFilePath = Path.GetFullPath(Path.Combine(Constants.Execution.Directory, $"{className}.java"));
        
        var codeWithDeps = await InstallJavaDependenciesIfAny(request.Code, request.Dependencies);

        await File.WriteAllTextAsync(javaFilePath, codeWithDeps);
        await ExecuteJavaCode(javaFilePath, sendSSEMessage);
    }

    private static async Task<string> InstallJavaDependenciesIfAny(string code, string[]? dependencies)
    {
        if (dependencies?.Length > 0)
        {
            var depDirectives = string.Join("\n", dependencies
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .Select(d => $"//DEPS {d}"));
        
            return $"{depDirectives}\n\n{code}";
        }

        return code;
    }

    private static async Task ExecuteJavaCode(string filePath, Func<string, Task> sendSSEMessage)
    {
        await sendSSEMessage("Executing Java code:\n");
        await ProcessRunner.RunAsync(JBangPath, $"run {filePath}", sendSSEMessage, timeoutMilliseconds:5000);
    }

    [GeneratedRegex(@"public\s+class\s+(\w+)")]
    private static partial Regex MyRegex();
}
