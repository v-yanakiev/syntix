using Agent.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Agent.Endpoints;

public static class HealthEndpoints
{
    public static IResult Handler()
    {
        return Results.Ok(new HealthResponse
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow
        });
    }
}