using System.Text.Json.Serialization;

namespace Agent.Models;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true)]
public record class ProcessResponse(string Message)
{
    public List<string> ProcessedFiles { get; init; } = new();
}

public record class HealthResponse
{
    public required string Status { get; init; }
    public DateTime Timestamp { get; init; }
}