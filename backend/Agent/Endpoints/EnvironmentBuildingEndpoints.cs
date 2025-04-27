using System.Text.Json;
using Agent.ExecutionEnvironmentTemplateCreation;
using Agent.Models;

namespace Agent.Endpoints;

public record BuildEnvironmentRequest(string AppName, string AppScopedFlyKey);
public record BuildEnvironmentResponse(string ImageUrl);

public static class EnvironmentBuildingEndpoints
{
    public static async Task<IResult> Handler(HttpContext context)
    {
        try
        {

            var requestData = (await JsonSerializer.DeserializeAsync(context.Request.Body,
                JsonSerializerContext.Default.BuildEnvironmentRequest))!;
            var imageUrl =
                await ExecutionEnvironmentTemplateCreator.Create(requestData.AppName, requestData.AppScopedFlyKey);
                
            return Results.Json(new BuildEnvironmentResponse(imageUrl),JsonSerializerContext.Default.BuildEnvironmentResponse);

        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to create environment template, due to the following error: " + e.Message);
            return Results.InternalServerError(e.Message);
        }
    }
}