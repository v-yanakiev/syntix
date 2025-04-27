using ExecutionEnvironmentTemplateCreator;
using FlyExecution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Models;
using Models.Enums;
using Models.Extensions;
Console.WriteLine("Enter number of templates to update: ");
var numberOfTemplatesToUpdate = int.Parse(Console.ReadLine()!);
var templateParams = new string[numberOfTemplatesToUpdate][];

var httpClient = new HttpClient();

for (int i = 0; i < numberOfTemplatesToUpdate; i++)
{
    
    Console.Write("Enter <label> <type> <DockerHub-image-name> [command]: ");
    var input = Console.ReadLine();
    var consoleArgs = input?.Split(' ', 4) ?? [];

    if (consoleArgs.Length < 3)
    {
        Console.WriteLine("Error: Must provide label, type, and DockerHub-image-name");
        return 1;
    }

    templateParams[i] = consoleArgs;
}
 
var tasks = Enumerable.Range(0, numberOfTemplatesToUpdate).Select(async i =>
{
    var consoleArgs= templateParams[i];
    
    var builder = Host.CreateApplicationBuilder();

    builder.Configuration
        .AddUserSecrets<Program>()
        .AddEnvironmentVariables();

    builder.Services.AddDbContext<PostgresContext>(options =>
    {
        var connectionString = builder.Configuration.GetConnectionString("Neon")
            .NormalizeConnectionString();

        options.UseNpgsql(connectionString);
    });
    var flyKey = builder.Configuration["FLY_ACCESS_TOKEN"]!;
    
    var host = builder.Build();


    try
    {
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PostgresContext>();
        var label = consoleArgs[0];
        var executionEnvironmentTypeString = consoleArgs[1];
        var creatorId= consoleArgs.Length>3? consoleArgs[3]:null;
        
        var parseSucceeded = Enum.TryParse(executionEnvironmentTypeString, true,
            out CodeExecutionEnvironment executionEnvironmentType);
        if (!parseSucceeded)
            throw new Exception($"Invalid execution environment type: {consoleArgs[1]}");

        var dockerHubImageName = consoleArgs[2];

#pragma warning disable CA1862
        var existingMachineTemplate =
            await db.FlyMachineTemplates.Include(a=>a.ExecutionMachines).FirstOrDefaultAsync(e =>
                e.Name.ToLower() == label.ToLower() && e.Type == executionEnvironmentTypeString);
#pragma warning restore CA1862Q

        if (existingMachineTemplate is not null)
        {
            Console.WriteLine("Environment already exists - removing.");
            var appsGeneratedFromTemplate=existingMachineTemplate.ExecutionMachines.Select(e=>e.AppName).ToList();

            await Task.WhenAll(appsGeneratedFromTemplate.Select(a =>
                FlyAppCleanupService.SendFlyAppDeleteRequestAsyncExplicitHttpClient(a, httpClient, flyKey)));
            
            if(existingMachineTemplate.AppName!=null)
                await FlyAppCleanupService.SendFlyAppDeleteRequestAsyncExplicitHttpClient(existingMachineTemplate.AppName, httpClient, flyKey);

            db.Remove(existingMachineTemplate);
        }

        string dockerfile;
        if (executionEnvironmentTypeString == "PostgreSQL")
        {
            dockerfile= $$"""
                          FROM vasil2000yanakiev/act:latest as agent
                              
                          FROM postgres
                          ENV POSTGRES_PASSWORD=mysecretpassword
                          COPY --from=agent /Agent /Agent

                          RUN chown postgres:postgres /Agent

                          COPY <<EOF /docker-cmd.sh
                          #!/bin/bash
                          /usr/local/bin/docker-entrypoint.sh postgres &
                          until pg_isready -h localhost; do
                            sleep 0.1
                          done
                          exec su postgres -c "/Agent"
                          EOF

                          RUN chmod +x /docker-cmd.sh
                          CMD ["/docker-cmd.sh"]
                          """;
        }
        else
        {
            dockerfile = $$"""
                           FROM vasil2000yanakiev/act:latest as agent

                           FROM {{dockerHubImageName}}
                           COPY --from=agent /Agent /Agent
                           CMD ["/Agent"]
                           """;
        }

        var dockerfilePath = Path.GetFullPath(Path.Combine("environments", label, "Dockerfile"));
        var directory = Path.GetDirectoryName(dockerfilePath)!;
        Directory.CreateDirectory(directory);
        await File.WriteAllTextAsync(dockerfilePath, dockerfile);
        
        var templateAppName=label.ToLower()+Guid.NewGuid();
        var launchResult = await FlyLauncher.ExecuteFlyLaunchAsync(label.ToLower()+Guid.NewGuid(), directory);

        if (launchResult.ImageName is null)
        {
            throw new InvalidOperationException("Failed to launch environment");
        }

        db.FlyMachineTemplates.Add(new ExecutionMachineTemplate()
            { Name = label, ImageUrl = launchResult.ImageName, Type = executionEnvironmentTypeString,CreatorId = creatorId, AppName =templateAppName });

        await db.SaveChangesAsync();

        Console.WriteLine($"Created environment: {executionEnvironmentType}");
        Console.WriteLine($"Dockerfile at: {dockerfilePath}");
        Console.WriteLine($"Image url: {launchResult.ImageName}");
        return 0;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
        return 1;
    }
}).ToList();

await Task.WhenAll(tasks);
return 0;