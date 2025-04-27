using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Agent.ExecutionEnvironmentTemplateCreation;

public static class FlyDeployer
{
    public record FlyDeployResult(
        bool Success,
        string? ImageUrl,
        string? ImageSize,
        string Output,
        string Error
    );

    public static async Task<FlyDeployResult> ExecuteFlyDeployAsync(
        string name,
        string workingDirectory,
        string flyKey,
        int vmMemory = 512,
        bool buildOnly = true,
        bool push = true,
        bool yes = true)
    {
        if (!Directory.Exists(workingDirectory))
        {
            throw new DirectoryNotFoundException($"Working directory not found: {workingDirectory}");
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = Constants.Fly.Location,
            Arguments = $"deploy --app {name} --vm-memory {vmMemory}" +
                       $"{(buildOnly ? " --build-only" : "")}" +
                       $"{(push ? " --push" : "")}" +
                       $"{(yes ? " -y" : "")}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = workingDirectory,
            Environment =
            {
                ["FLY_ACCESS_TOKEN"] = flyKey
            }
        };

        using var process = new Process { StartInfo = startInfo };
        var output = new System.Text.StringBuilder();
        var error = new System.Text.StringBuilder();

        process.OutputDataReceived += (sender, args) => 
        {
            if (args.Data != null)
                output.AppendLine(args.Data);
        };
        process.ErrorDataReceived += (sender, args) => 
        {
            if (args.Data != null)
                error.AppendLine(args.Data);
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        await process.WaitForExitAsync();

        var outputStr = output.ToString();
        var errorStr = error.ToString();

        // Search for image information in both output and error streams
        var combinedOutput = outputStr + "\n" + errorStr;
        
        // Using more precise regex patterns that match the actual output format
        var imageMatch = Regex.Match(combinedOutput, @"image: (registry\.fly\.io/[^:\s]+:[^\s]+)");
        var imageSizeMatch = Regex.Match(combinedOutput, @"image size: (\d+ MB)");

        return new FlyDeployResult(
            Success: process.ExitCode == 0,
            ImageUrl: imageMatch.Success ? imageMatch.Groups[1].Value.Trim() : null,
            ImageSize: imageSizeMatch.Success ? imageSizeMatch.Groups[1].Value.Trim() : null,
            Output: outputStr,
            Error: errorStr
        );
    }
}
