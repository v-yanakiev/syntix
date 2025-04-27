using System.Text;
using AbstractExecution;
using LocalExecution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Models;
using Models.Enums;
using Models.Extensions;

namespace LocalExecutionTesting;

public class LocalCodeExecutorIntegrationTests
{
    private readonly CodeExecutor _executor;
    private readonly PostgresContext _dbContext;

    public LocalCodeExecutorIntegrationTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<LocalCodeExecutorIntegrationTests>()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<PostgresContext>();
        var connectionString = configuration.GetConnectionString("Neon").NormalizeConnectionString();
        optionsBuilder.UseNpgsql(connectionString);
        _dbContext = new PostgresContext(optionsBuilder.Options);

        _executor = new CodeExecutor(new HttpClient(), new LocalAgentSetup(_dbContext));
    }

    [Fact]
    public async Task ExecuteCodeAsync_NodeJs_ReturnsExpectedOutput()
    {
        const CodeExecutionEnvironment codeExecutionEnvironment = CodeExecutionEnvironment.NodeJS;
        // Arrange
        var parameters = new CodeExecutionParameters("console.log('Hello, World!');", codeExecutionEnvironment,"js");
        
        // Act
        var outputBuilder = new StringBuilder();
        await foreach (var item in _executor.ExecuteCodeAsync(parameters, 0))
            outputBuilder.Append(item);
        var result = outputBuilder.ToString();

        // Assert
        Assert.Contains("Hello, World!", result);
    }

    [Fact]
    public async Task ExecuteCodeAsync_NodeTs_ReturnsExpectedOutput()
    {
        const CodeExecutionEnvironment codeExecutionEnvironment = CodeExecutionEnvironment.NodeTS;

        // Arrange
        var parameters = new CodeExecutionParameters(@"
                    interface Greeting {
                        message: string;
                    }
                    
                    const greeting: Greeting = { message: 'Hello, TypeScript!' };
                    console.log(greeting.message);
                ", codeExecutionEnvironment,"ts");
        var machineTemplate =
            await _dbContext.FlyMachineTemplates.FirstAsync(a => a.Type == codeExecutionEnvironment.ToString());

        // Act
        var outputBuilder = new StringBuilder();
        await foreach (var item in _executor.ExecuteCodeAsync(parameters, 0))
            outputBuilder.Append(item);
        var result = outputBuilder.ToString();

        // Assert
        Assert.Contains("Hello, TypeScript!", result);
    }

    [Fact]
    public async Task ExecuteCodeAsync_WithDependencies_InstallsAndExecutesCorrectly()
    {
        const CodeExecutionEnvironment codeExecutionEnvironment = CodeExecutionEnvironment.NodeJS;

        // Arrange
        var parameters = new CodeExecutionParameters(@"
                    import moment from 'moment';
                    console.log(moment().format('YYYY-MM-DD'));
                ", codeExecutionEnvironment, "js",["moment"]);

        var machineTemplate =
            await _dbContext.FlyMachineTemplates.FirstAsync(a => a.Type == codeExecutionEnvironment.ToString());

        // Act
        var outputBuilder = new StringBuilder();
        await foreach (var item in _executor.ExecuteCodeAsync(parameters, 0))
            outputBuilder.Append(item);
        var result = outputBuilder.ToString();

        // Assert
        Assert.Matches(@"\d{4}-\d{2}-\d{2}", result.Trim());
    }

    [Fact]
    public async Task ExecuteCodeAsync_WithSyntaxError_ReturnsErrorMessage()
    {
        const CodeExecutionEnvironment codeExecutionEnvironment = CodeExecutionEnvironment.NodeJS;

        // Arrange
        var parameters = new CodeExecutionParameters(
            "console.log('Unclosed string);", codeExecutionEnvironment,"js");

        var machineTemplate =
            await _dbContext.FlyMachineTemplates.FirstAsync(a => a.Type == codeExecutionEnvironment.ToString());

        // Act
        var outputBuilder = new StringBuilder();
        await foreach (var item in _executor.ExecuteCodeAsync(parameters,0))
            outputBuilder.Append(item);
        var result = outputBuilder.ToString();

        // Assert
        Assert.Contains("SyntaxError", result);
    }

    [Fact]
    public async Task ExecuteCodeAsync_LongRunningTask_ReturnsIntermediateResults()
    {
        const CodeExecutionEnvironment codeExecutionEnvironment = CodeExecutionEnvironment.NodeJS;

        // Arrange
        var parameters = new CodeExecutionParameters(@"
                    for(let i = 1; i <= 5; i++) {
                        console.log(`Step ${i}`);
                        await new Promise(resolve => setTimeout(resolve, 1000));
                    }
                ", codeExecutionEnvironment,"js");
        var machineTemplate =
            await _dbContext.FlyMachineTemplates.FirstAsync(a => a.Type == codeExecutionEnvironment.ToString());

        // Act
        var steps = new List<string>();
        await foreach (var message in _executor.ExecuteCodeAsync(parameters,0))
            if (message.StartsWith("Step"))
                steps.Add(message.Trim());

        // Assert
        Assert.Equal(5, steps.Count);
        for (var i = 1; i <= 5; i++) Assert.Contains($"Step {i}", steps);
    }
}