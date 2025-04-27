using System.Diagnostics;
using System.Text.Json;
using Agent.Endpoints;
using Agent.Models;
using Microsoft.AspNetCore.Http.Features;

var stopwatch = Stopwatch.StartNew();

Console.WriteLine("Version: Mar 20, 08:52");

var customRootDirectory = Environment.GetEnvironmentVariable("USER_ROOT_DIRECTORY");

if (!string.IsNullOrWhiteSpace(customRootDirectory))
{
    Console.WriteLine("CUSTOM ENVIRONMENT");
    Agent.Constants.Execution.Directory = customRootDirectory;
}
else
{
    Console.WriteLine("NOT CUSTOM ENVIRONMENT");
    if (Directory.Exists(Agent.Constants.Execution.Directory))
    {
        Directory.Delete(Agent.Constants.Execution.Directory, recursive: true);
    }
}

Directory.CreateDirectory(Agent.Constants.Execution.Directory);

var builder = WebApplication.CreateSlimBuilder(args);
GlobalState.IsProduction = builder.Environment.IsProduction();

builder.WebHost.UseUrls("http://[::]:65432");

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, JsonSerializerContext.Default);
});


// Configure form options
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = long.MaxValue; // Adjust as needed
    options.ValueLengthLimit = int.MaxValue;
    options.MemoryBufferThreshold = int.MaxValue;
});

var app = builder.Build();

app.Use(async (context, next) =>
{
    var feature = context.Features.Get<IHttpMaxRequestBodySizeFeature>();
    if (feature != null) feature.MaxRequestBodySize = null;
    
    await next();
});

app.MapPost("/execute", CodeExecutionEndpoints.Handler);

app.MapGet("/scanDirectory", DirectoryScanningEndpoints.Handler);

app.MapGet("/downloadFile", FileDownloadEndpoints.Handler);

app.MapPost("/saveFiles", FileUploadEndpoints.Handler).DisableAntiforgery();

app.MapGet("/health", HealthEndpoints.Handler);

app.MapPost("/buildEnvironment", (Delegate)EnvironmentBuildingEndpoints.Handler);

app.Lifetime.ApplicationStarted.Register(() =>
{
    stopwatch.Stop();
    Console.WriteLine($"AGENT STARTED AFTER {stopwatch.ElapsedMilliseconds}ms.");
});

app.Run();
public static class GlobalState
{
    public static bool Initialized { get; set; }
    public static bool IsProduction { get; set; }
}

