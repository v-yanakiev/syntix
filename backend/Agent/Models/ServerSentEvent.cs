//must be the same data model as AbstractExecution/SseEvent

namespace Agent.Models;

public record ServerSentEvent(ServerSentMessage Data);

public record ServerSentMessage(string Text);