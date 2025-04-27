using System.Text.Json;
using System.Text.Json.Serialization;
using Models.Enums;

namespace AbstractExecution;

public record CodeExecutionParameters(
    string Code,
    CodeExecutionEnvironment Environment,
    string LanguageIdentifier,
    string[]? Dependencies = null,
    bool? DoNotCreateNewFile = null)
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
    public static CodeExecutionParameters Parse(string json)
    {
        var parameters = JsonSerializer.Deserialize<CodeExecutionParameters>(json, Options)
                         ?? throw new JsonException("Failed to deserialize CodeExecutionParameters");

        parameters = parameters with { Code = parameters.Code ?? "", LanguageIdentifier = parameters.LanguageIdentifier ?? ""};
        return parameters;
    }


    public string GetUserVisibleSummary()
    {
        //used when the AI doesn't return the code, only the function call
        var dependenciesSummary = Dependencies?.Length>0
            ? ($"Dependencies that will be installed:\n```\n"
               + string.Join(", ", Dependencies) +
               "\n```\n")
            : "";

        return $"Code:\n```{LanguageIdentifier.ToLower()}\n"
               + Code +
               "\n```\n" +
               dependenciesSummary +
               "Executing...";
    }
}