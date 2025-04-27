namespace Agent.CodeExecution.Executors;

public static class Common
{
    public static async Task LogDependencies(string[] dependencies, Func<string, Task> sendSSEMessage)
    {
        await sendSSEMessage("Dependencies list:\n");
        await sendSSEMessage(string.Join(", ", dependencies) + "\n");
    }
}