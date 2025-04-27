using System.Diagnostics;

namespace FlyExecution;

public static class IpsAllocator
{
    public static async Task<string> AllocatePrivateIpv6Async(string appName, string flyKey, string flyctlPath)
    {
        if (string.IsNullOrEmpty(flyKey))
            throw new ArgumentNullException(nameof(flyKey));

        var startInfo = new ProcessStartInfo
        {
            FileName = flyctlPath,
            Arguments = $"ips allocate-v6 -a {appName} --private",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            Environment =
            {
                ["FLY_ACCESS_TOKEN"] = flyKey
            }
        };

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        var output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync();
            throw new Exception($"Failed to allocate IP. Exit code: {process.ExitCode}. Error: {error}");
        }

        var ipLine = output.Split('\n')
            .FirstOrDefault(line => line.Contains("v6"))
            ?.Split('\t', StringSplitOptions.RemoveEmptyEntries);

        if (ipLine == null || ipLine.Length < 2)
        {
            throw new Exception($"Failed to parse IP from output: {output}");
        }

        return ipLine[1].Trim();
    }
}