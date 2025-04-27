using System.Text.Json.Serialization;
using Agent.Endpoints;

namespace Agent.Models;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    GenerationMode = JsonSourceGenerationMode.Default,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    Converters = [typeof(JsonStringEnumConverter<CodeExecutionEnvironment>)])]
[JsonSerializable(typeof(ExecutionRequest))]
[JsonSerializable(typeof(ServerSentEvent))]
[JsonSerializable(typeof(ProcessResponse))]
[JsonSerializable(typeof(HealthResponse))]
[JsonSerializable(typeof(DirectoryResponse))]
[JsonSerializable(typeof(FileSystemNode))]
[JsonSerializable(typeof(List<FileSystemNode>))]
[JsonSerializable(typeof(BuildEnvironmentRequest))]
[JsonSerializable(typeof(BuildEnvironmentResponse))]
public partial class JsonSerializerContext : System.Text.Json.Serialization.JsonSerializerContext
{
}