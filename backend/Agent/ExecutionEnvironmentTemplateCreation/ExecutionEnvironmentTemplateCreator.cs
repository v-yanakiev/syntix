namespace Agent.ExecutionEnvironmentTemplateCreation;

public static class ExecutionEnvironmentTemplateCreator
{
    public static async Task<string> Create(string appName, string flyKey)
    {
        var userDockerfilePath = Path.GetFullPath(Constants.Execution.Directory + "/Dockerfile");
        var flyTomlPath = Path.GetFullPath(Constants.Execution.Directory + "/fly.toml");
        var userDockerfileContent = await File.ReadAllTextAsync(userDockerfilePath);
        var correctDockerfileContent = GetDockerfileWithInjectedAgent(userDockerfileContent);
        await File.WriteAllTextAsync(userDockerfilePath, correctDockerfileContent);

        var flyTomlContent = GetFlyTomlContent(appName);
        
        await File.WriteAllTextAsync(flyTomlPath, flyTomlContent);
        
        var launchResult =
            await FlyDeployer.ExecuteFlyDeployAsync(appName, Constants.Execution.Directory, flyKey);
        if (launchResult.ImageUrl is null)
        {
            throw new InvalidOperationException($"Failed to launch environment, with error: {launchResult.Error}");
        }

        return launchResult.ImageUrl;
    }

    private static string GetFlyTomlContent(string appName)
    {
        var flyTomlContent = $@"
app = '{appName}'
primary_region = 'fra'

[build]

[http_service]
  internal_port = 8080
  force_https = true
  auto_stop_machines = 'stop'
  auto_start_machines = true
  min_machines_running = 0
  processes = ['app']

[[vm]]
  memory = '512mb'
  cpu_kind = 'shared'
  cpus = 1";
        return flyTomlContent;
    } 

    private static string GetDockerfileWithInjectedAgent(string userDockerfile)
    {
        // Filter out CMD and ENTRYPOINT instructions
        var filteredUserDockerfile = FilterCmdAndEntrypoint(userDockerfile);

        // Create the final Dockerfile with agent injection
        var finalDockerfile= $@"FROM vasil2000yanakiev/act:latest as agent

{filteredUserDockerfile}
COPY --from=agent /Agent /Agent
WORKDIR /
ENTRYPOINT []
CMD [""./Agent""]";
        
        return finalDockerfile;
    }

    private static string FilterCmdAndEntrypoint(string dockerfile)
    {
        var filteredLines = new List<string>();
        var lines = dockerfile.Split(new[] { '\r', '\n' }, StringSplitOptions.None);
        var skipNextLine = false;
        foreach (var t in lines)
        {
            var trimmedLine = t.TrimStart();

            // Check if this line starts a CMD or ENTRYPOINT instruction
            var isInstruction = trimmedLine.StartsWith("CMD ", StringComparison.OrdinalIgnoreCase) ||
                                trimmedLine.StartsWith("ENTRYPOINT ", StringComparison.OrdinalIgnoreCase) ||
                                trimmedLine.Equals("CMD", StringComparison.OrdinalIgnoreCase) ||
                                trimmedLine.Equals("ENTRYPOINT", StringComparison.OrdinalIgnoreCase);
            if (skipNextLine)
            {
                // Check if we need to continue skipping (for multi-line instructions)
                skipNextLine = t.TrimEnd().EndsWith("\\");
                continue;
            }

            if (isInstruction)
            {
                // Skip this instruction and check if it continues to next line
                skipNextLine = t.TrimEnd().EndsWith("\\");
            }
            else
            {
                filteredLines.Add(t);
            }
        }

        // Join the filtered lines, preserving original line endings
        return string.Join(Environment.NewLine, filteredLines);
    }
}