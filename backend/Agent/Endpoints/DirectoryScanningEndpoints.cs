using Agent.Models;
using System.Text.Json;

namespace Agent.Endpoints;


public static class DirectoryScanningEndpoints
{
    public static IResult Handler(CancellationToken cancellationToken)
    {
        return Results.Stream(StreamResponse, "text/event-stream");

        async Task StreamResponse(Stream stream)
        {
            await using var writer = new StreamWriter(stream);

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var structure = ScanDirectory(Constants.Execution.Directory);
                    var response = new DirectoryResponse("Directory scanned successfully")
                    {
                        Structure = structure
                    };

                    var json = JsonSerializer.Serialize(response, JsonSerializerContext.Default.DirectoryResponse);
                    await writer.WriteLineAsync($"data: {json}");
                    await writer.WriteLineAsync();
                    await writer.FlushAsync(cancellationToken);

                    await Task.Delay(500, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Normal cancellation, no need to handle
            }
            catch (Exception ex)
            {
                var errorResponse = new DirectoryResponse($"Error: {ex.Message}");
                var errorJson =
                    JsonSerializer.Serialize(errorResponse, JsonSerializerContext.Default.DirectoryResponse);
                await writer.WriteLineAsync($"data: {errorJson}");
                await writer.WriteLineAsync();
                await writer.FlushAsync(cancellationToken);
            }
        }
    }

    private static FileSystemNode ScanDirectory(string path)
    {
        var dirInfo = new DirectoryInfo(path);
        var node = new FileSystemNode(dirInfo.Name, []);

        try
        {
            var directories = dirInfo.GetDirectories();

            foreach (var dir in directories)
            {
                node.Children!.Add(ScanDirectory(dir.FullName));
            }

            foreach (var file in dirInfo.GetFiles())
            {
                node.Children!.Add(new FileSystemNode(file.Name));
            }
        }
        catch (Exception)
        {
            var emptyNode = new FileSystemNode(dirInfo.Name, []);
            return emptyNode;
        }
        
        return node;

    }
}