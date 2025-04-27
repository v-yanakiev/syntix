using Models.Enums;

namespace AbstractExecution;

public class ExecutionConstants
{
    private static readonly string[] AllowedEnvironments =
        Enum.GetNames<CodeExecutionEnvironment>()
            .Where(a => a != CodeExecutionEnvironment.EnvironmentDefining.ToString())
            .ToArray();

    public const string CodeExecutionFunctionName = "execute_code";

    public const string CodeExecutionFunctionDescription =
        "Execute code and record the outcome for validation purposes.";

    public static readonly string CodeExecutionFunctionParameters =
        $$"""
          {
              "type": "object",
              "required": ["code", "environment","dependencies", "languageIdentifier"],
              "properties": {
                  "code": {
                      "type": "string",
                      "description": "The code to be executed (as raw text with preserved formatting and line breaks). The code must contain at least 3 unit tests and must print the result of those tests to the console. Input should preserve all whitespace and newlines without escaping."
                  },
                  "environment": {
                      "type": "string",
                      "enum": {{System.Text.Json.JsonSerializer.Serialize(AllowedEnvironments)}},
                      "description": "The environment in which to execute the code."
                  },
                  "dependencies": {
                      "type": "array",
                      "items": {
                          "type": "string"
                      },
                      "description": "List of package dependencies required by the code (e.g. ['lodash'] or ['org.junit.jupiter:junit-jupiter:5.9.2'])"
                  },
                  "languageIdentifier": {
                      "type": "string",
                      "description": "A string identifier that specifies the programming language for syntax highlighting in Markdown code blocks. Common examples include: 'python', 'javascript', 'java', 'csharp', 'cpp', etc."
                  }
              }
          }
          """;
}