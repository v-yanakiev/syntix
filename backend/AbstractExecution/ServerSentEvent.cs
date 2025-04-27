//must be the same data model as CodeRunner/Models/SseEvent

using System.Text.Json;

namespace AbstractExecution;

public record ServerSentEvent(ServerSentMessage Data)
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public static ServerSentEvent? Parse(string json)
    {
        var parameters = JsonSerializer.Deserialize<ServerSentEvent>(json, Options);
        return parameters;
    }
}

public record ServerSentMessage(string Text);