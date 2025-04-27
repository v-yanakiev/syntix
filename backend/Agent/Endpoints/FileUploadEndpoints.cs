using Agent.Constants;
using Agent.Models;
namespace Agent.Endpoints;

public static class FileUploadEndpoints
{
    public static async Task<IResult> Handler(IFormFileCollection files)
    {
        if (files.Count == 0)
        {
            return Results.BadRequest(new ProcessResponse("No files received"));
        }

        var processedFiles = new List<string>();

        try
        {
            foreach (var file in files)
            {
                var relativePath = file.FileName.Replace("\\", "/");
                var fullPath = Path.Combine(Execution.Directory, relativePath);
            
                var directory = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                await using var fileStream = File.Create(fullPath);
                await using var uploadStream = file.OpenReadStream();
                await uploadStream.CopyToAsync(fileStream);
            
                processedFiles.Add(relativePath);
            }

            return Results.Ok(new ProcessResponse("Files processed successfully")
            {
                ProcessedFiles = processedFiles
            });
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error processing files",
                detail: ex.Message,
                statusCode: 500);
        }
    }
}