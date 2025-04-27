using Agent.Models;
using Agent.OS;

namespace Agent.CodeExecution.Executors;

public static class PostgreSQLExecutor
{
    public static async Task Execute(ExecutionRequest request, Func<string, Task> sendSSEMessage)
    {
        if (!GlobalState.Initialized)
        {
            GlobalState.Initialized = true;
        }

        var sqlFilePath = Path.Combine(Constants.Execution.Directory, "query.sql");
        if (request.DoNotCreateNewFile != true)
        {
            await File.WriteAllTextAsync(sqlFilePath, request.Code);
        }

        await ConfigurePostgreSQLIfNeeded(request.Dependencies, sendSSEMessage);
        await ExecuteSQLCode(sendSSEMessage);
    }

    private static async Task ExecuteSQLCode(Func<string, Task> sendSSEMessage)
    {
        await sendSSEMessage("Result from the execution of the SQL query:\n");
        await ProcessRunner.RunAsync("psql", "-f query.sql", sendSSEMessage);
    }
    
    private static async Task ConfigurePostgreSQLIfNeeded(string[]? dependencies, Func<string, Task> sendSSEMessage)
    {
        if (dependencies?.Length > 0)
        {
            await Common.LogDependencies(dependencies, sendSSEMessage);
            await sendSSEMessage("Configuring PostgreSQL extensions...\n");
            
            foreach (var extension in dependencies)
            {
                await ProcessRunner.RunAsync("psql", $"-c 'CREATE EXTENSION IF NOT EXISTS {extension};'", sendSSEMessage);
            }
        }
    }
}