using System.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace Tooling;

public class FlyKeyTool(IConfiguration configuration)
{
    private readonly string _flyKey = configuration["FLY_ACCESS_TOKEN"] ?? throw new Exception("FLY_ACCESS_TOKEN not configured");
    private readonly string _flyCtlPath = configuration["FLYCTL_PATH"] ?? throw new Exception("FLYCTL_PATH not configured");

    public async Task<string> GetAppScopedKey(string appName)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = _flyCtlPath,
            Arguments = $"tokens create deploy -a {appName} --expiry 1h",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            Environment =
            {
                ["FLY_ACCESS_TOKEN"] = _flyKey
            }
        };

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        var output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync();
            throw new Exception($"Failed to get deploy token. Exit code: {process.ExitCode}. Error: {error}");
        }

        // Trim any whitespace and return the token
        var token = output.Trim();
    
        if (string.IsNullOrEmpty(token))
        {
            throw new Exception("Deploy token is empty");
        }

        return token;
    }
}