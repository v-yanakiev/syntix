using Agent.Models;

namespace Agent.Endpoints;

public static class FileDownloadEndpoints
{
    public static IResult Handler(string pathToFile)
    {
        try
        {
            var fullPath =Path.GetFullPath(Path.Combine(Constants.Execution.Directory, pathToFile.Replace("\\", "/")));
            
            if (!Path.Exists(fullPath) || File.GetAttributes(fullPath).HasFlag(FileAttributes.Directory))
            {
                return Results.NotFound(new ProcessResponse($"File not found: {pathToFile}"));
            }

            return Results.File(fullPath, enableRangeProcessing:true);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error processing download request",
                detail: ex.Message,
                statusCode: 500);
        }
    }
}