using System.Text.Json;
using System.Text.Json.Serialization;

namespace AbstractExecution;

public static class ToMachineParameterSerializer
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    public static string Serialize(object value)
    {
        return JsonSerializer.Serialize(value, JsonSerializerOptions);
    }
}