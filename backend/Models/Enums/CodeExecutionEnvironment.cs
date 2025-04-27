//must be the same as CodeExecutionEnvironment in CodeRunner

using System.Text.Json.Serialization;

namespace Models.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CodeExecutionEnvironment
{
    NodeJS,
    NodeTS,
    CSharp,
    Java,
    Python,
    Go,
    Rust,
    PostgreSQL,
    EnvironmentDefining,
    Custom
}