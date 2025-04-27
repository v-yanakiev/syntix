using System.Text.Json.Serialization;

namespace Agent.Models;

//must be the same as CodeExecutionEnvironment in AbstractExecution
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
    EnvironmentCreating,
    Custom
}

public record ExecutionRequest(
    [property: JsonRequired] CodeExecutionEnvironment Environment,
    [property: JsonRequired] string Code,
    string[]? Dependencies,
    bool? DoNotCreateNewFile,
    string? CodeFile,
    string? AfterChangesValidationCommand,
    string? DependencyInstallingTerminalCall);