using System.Data;
using System.Net.Http.Headers;
using System.Text.Json;
using AbstractExecution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Models;
using Models.Enums;

namespace FlyExecution;

public record FlyMachineCreationResponse(string? Id, string? State);

public record FlyMachineStartResponse(string State);

public class FlyMachineSetup : IExecutionSetup
{
    private readonly string _flyKey;

    private readonly PostgresContext _postgresContext;
    private readonly HttpClient _httpClient;
    private readonly IDbContextFactory<PostgresContext> _contextFactory;
    private readonly IConfiguration _configuration;

    public FlyMachineSetup(PostgresContext postgresContext, HttpClient httpClient,
        IDbContextFactory<PostgresContext> contextFactory, IConfiguration configuration)
    {
        _flyKey = configuration["FLY_ACCESS_TOKEN"] ?? throw new Exception("FLY_ACCESS_TOKEN not configured");

        _postgresContext = postgresContext;

        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _flyKey);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        _contextFactory = contextFactory;
        _configuration = configuration;
    }

    private const int NumberOfInactiveMachinesBelowWhichToCreate = 3;
    // private const int OptimalNumberOfMachines = 4;
    // private const int NumberOfInactiveMachinesAboveWhichToDelete = 6;

    public async Task<AgentInfo> GetAgentInfoAsync(long executionEnvironmentTemplateId, Guid? chatId, string userId)
    {
        var template = await GetTemplateAsync(executionEnvironmentTemplateId);

        var machine = await GetSameChatStartedMachineAsync(template, chatId, userId);

        var machineUrl = GetMachineUrl(machine.AppAddress);
        var agentInfo = new AgentInfo(template, machine, machineUrl);
        return agentInfo;
    }

    private string GetMachineUrl(string appAddress)
    {
        var machineUrl = $"http://[{appAddress}]:65432";
        return machineUrl;
    }
    
    public async Task InitializeMachineAsync(long executionEnvironmentTemplateId, Guid? chatId, string userId)
    {
        try
        {

            var template = await GetTemplateAsync(executionEnvironmentTemplateId);
            var alreadyStartedMachine = await TryGetSameChatStartedMachineAsync(template, chatId, userId);

            if (alreadyStartedMachine != null)
            {
                return;
            }

            _ = StopOtherUserActiveMachines(chatId, userId, template.Id);

            _ = await StartFirstStoppedMachineAsync(template, chatId, userId) ??
                await CreateMachineAsync(template, chatId, userId);

            _ = EnsureEnoughMachinesExistInPool(template);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to initialize machine with executionEnvironmentTemplateId={executionEnvironmentTemplateId}, chatId={chatId}, userId={userId}, due to the following error: {e.Message}, with the following stackTrace: {e.StackTrace}");
            throw;
        }
    }

    public async Task StopMachinesAssociatedWithChatAsync(Guid chatId, CancellationToken stoppingToken)
    {
        // Get all machines associated with the chat
        var machines = await _postgresContext.ExecutionMachine.Where(a => a.ChatId == chatId)
            .ToListAsync(stoppingToken);

        await FlyMachineCleanupService.StopMachines(machines, _httpClient, _postgresContext, stoppingToken);
    }
    private async Task StopOtherUserActiveMachines(Guid? chatId, string userId, long? executionMachineTemplateId)
    {
        await using var scopedContext = await _contextFactory.CreateDbContextAsync();

        var otherMachines = await scopedContext.ExecutionMachine
            .Where(a => a.UserId == userId 
                        && a.ExecutionMachineTemplate!.Type!=nameof(CodeExecutionEnvironment.EnvironmentDefining) &&  // we don't want to stop the machine builders
                        (a.ChatId != chatId ||
                         a.ExecutionMachineTemplateId !=
                         executionMachineTemplateId)) // the actually requested machine, the address of which will be returned, should not be stopped 
            .ToListAsync();

        await FlyMachineCleanupService.StopMachines(otherMachines, _httpClient, scopedContext);
    }

    private async Task<ExecutionMachine> GetSameChatStartedMachineAsync(ExecutionMachineTemplate template, Guid? chatId, string userId)
    {
        ExecutionMachine? machine;

        var retryCounter = 0;
        while ((machine = await TryGetSameChatStartedMachineAsync(template, chatId, userId)) == null)
        {
            retryCounter++;
            if (retryCounter == 5)
            {
                throw new Exception("Machine failed to start!");
            }
            await Task.Delay(2000);
        }

        return machine;
    }

    private async Task<ExecutionMachine?> TryGetSameChatStartedMachineAsync(ExecutionMachineTemplate template,
        Guid? chatId, string userId)
    {
        var machine = await _postgresContext.ExecutionMachine.FirstOrDefaultAsync(a =>
            a.ExecutionMachineTemplate == template && (chatId == null || a.ChatId == chatId)&&a.UserId == userId);
        return machine;
    }

    private async Task<ExecutionMachine?> StartFirstStoppedMachineAsync(ExecutionMachineTemplate template, Guid? chatId, string userId)
    {
        ExecutionMachine? machine = null;
        
        // First, try to acquire the machine within a short transaction
        await using (var transaction = await _postgresContext.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead))
        {
            try
            {
                machine = await _postgresContext.ExecutionMachine
                    .FromSql($"SELECT * FROM execution_machine WHERE execution_machine_template_id = {template.Id} AND last_active IS NULL FOR UPDATE SKIP LOCKED LIMIT 1")
                    .FirstOrDefaultAsync();

                if (machine != null)
                {
                    // Mark the machine as "in progress" with a timestamp
                    machine.LastActive = DateTime.UtcNow;
                    machine.ChatId = chatId;
                    machine.UserId = userId;
                    await _postgresContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        if (machine == null) return null;

        try
        {
            // Perform the time-consuming operations outside the transaction
            await StartMachine(machine.Id, machine.AppName);
            await WaitForMachineStarted(machine.AppName, machine.Id, GetMachineUrl(machine.AppAddress));

            // Update the status in a new transaction
            await using var updateTransaction = await _postgresContext.Database.BeginTransactionAsync();
            try
            {
                var logMessage = $"SUCCESS 7: Started machine at time {DateTime.UtcNow}, with id {machine.Id} and app name {machine.AppName}, requested by user {machine.UserId}.";
                _postgresContext.Logs.Add(new Log { Content = logMessage });
                await _postgresContext.SaveChangesAsync();
                await updateTransaction.CommitAsync();
            }
            catch
            {
                await updateTransaction.RollbackAsync();
                throw;
            }

            return machine;
        }
        catch
        {
            // If starting the machine fails, you might want to release it
            await ReleaseMachine(machine.Id);
            throw;
        }
    }

    private async Task ReleaseMachine(string machineId)
    {
        await using var transaction = await _postgresContext.Database.BeginTransactionAsync();
        try
        {
            var machine = await _postgresContext.ExecutionMachine
                .FromSql($"SELECT * FROM execution_machine WHERE id = {machineId} FOR UPDATE")
                .FirstOrDefaultAsync();

            if (machine != null)
            {
                machine.LastActive = null;
                machine.ChatId = null;
                machine.UserId = null;
                await _postgresContext.SaveChangesAsync();
            }
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task<ExecutionMachineTemplate> GetTemplateAsync(long executionEnvironmentTemplateId)
    {
        var template = await _postgresContext.FlyMachineTemplates.Include(a => a.ExecutionMachines)
            .FirstAsync(a => a.Id==executionEnvironmentTemplateId);
        return template;
    }

    private async Task EnsureEnoughMachinesExistInPool(ExecutionMachineTemplate template)
    {
        if (template.Type == CodeExecutionEnvironment.Custom.ToString())
        {
            return;
        }
        
        var numberOfMachinesInPool = template.ExecutionMachines.Count(a => a.LastActive == null);
        if (numberOfMachinesInPool >= NumberOfInactiveMachinesBelowWhichToCreate)
        {
            return;
        }

        var numberOfMachinesToCreate = NumberOfInactiveMachinesBelowWhichToCreate - numberOfMachinesInPool;
        var taskContextPairs = new List<(Task<ExecutionMachine> Task, PostgresContext Context)>();

        // Start all tasks
        for (var i = 0; i < numberOfMachinesToCreate; i++)
        {
            var scopedContext = await _contextFactory.CreateDbContextAsync();
            var task = CreateMachineAsync(template, injectedPostgresContext: scopedContext);
            taskContextPairs.Add((task, scopedContext));
        }

        // Await all tasks and dispose contexts
        foreach (var (task, context) in taskContextPairs)
        {
            try
            {
                await task;
            }
            finally
            {
                await context.DisposeAsync();
            }
        }
    }

    public async Task<ExecutionMachine> CreateMachineAsync(ExecutionMachineTemplate machineTemplate,
        Guid? chatId = null, string? activeUserId = null, PostgresContext? injectedPostgresContext = null)
    {
        var startMachine = activeUserId != null;
        var actualPostgresContext = injectedPostgresContext ?? _postgresContext;

        var appNamePrefix = machineTemplate.Type == CodeExecutionEnvironment.Custom.ToString() ? "c" : "a";
        var appName = appNamePrefix + Guid.NewGuid().ToString().Substring(1);
        
        await CreateFlyAppAsync(_httpClient, appName.Replace("-", ""), appName);
        var appAddress = await IpsAllocator.AllocatePrivateIpv6Async(appName, _flyKey, _configuration["FLYCTL_PATH"]!);
        var machinePerformanceConfig = machineTemplate.Type == CodeExecutionEnvironment.EnvironmentDefining.ToString()
            ? new
            {
                cpu_kind = "shared",
                cpus = 1,
                memory_mb =  256
            }
            : new
            {
                cpu_kind = "shared",
                cpus = 4,
                memory_mb = 1024
            };
        
        var machineConfig = new
        {
            name = appName,
            skip_launch = !startMachine,
            config = new
            {
                image = machineTemplate.ImageUrl,
                env = new { APP_ENV = "production", USER_ROOT_DIRECTORY = machineTemplate.RootDirectory },
                services = (object[])
                [
                    new
                    {
                        internal_port = 65432,
                        autostop = "off",
                        autostart = true,
                        ports = (object[]) [new { port = 65432 }]
                    }
                ],
                auto_destroy = false,
                guest = machinePerformanceConfig
            }
        };
        var machineConfigJson = JsonSerializer.Serialize(machineConfig);
        var content = new StringContent(machineConfigJson, System.Text.Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"https://api.machines.dev/v1/apps/{appName}/machines", content);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorResponseContent = await response.Content.ReadAsStringAsync();
            var errorCode = response.StatusCode;
            throw new Exception(
                $"Fly machine creation failed with error code: {errorCode} and response content: {errorResponseContent}");
        }
        
        var resultJson = await response.Content.ReadAsStringAsync();
        var machineCreationResponse =
            JsonSerializer.Deserialize<FlyMachineCreationResponse>(resultJson, JsonSerializerOptions.Web);
        if (machineCreationResponse?.Id == null)
        {
            throw new Exception("Failed to get machine ID from Fly API response");
        }

        if (startMachine)
        {
            await WaitForMachineStarted(appName, machineCreationResponse.Id, GetMachineUrl(appAddress));

            var logMessage =
                $"SUCCESS 5: Created and started machine at time {DateTime.UtcNow}, with id {machineCreationResponse.Id} and app name {appName}, requested by user {activeUserId}.";
            actualPostgresContext.Logs.Add(new Log() { Content = logMessage });
        }

        var executionMachine = new ExecutionMachine
        {
            Id = machineCreationResponse.Id,
            LastActive = activeUserId == null ? null : DateTime.UtcNow,
            ExecutionMachineTemplateId = machineTemplate.Id,
            CreatedAt = DateTime.UtcNow,
            ChatId = chatId,
            UserId = activeUserId,
            AppName = appName,
            AppAddress = appAddress
        };
        actualPostgresContext.ExecutionMachine.Add(executionMachine);
        await actualPostgresContext.SaveChangesAsync();
        return executionMachine;

    }

    public static async Task CreateFlyAppAsync(HttpClient httpClient, string? networkName, string appName)
    {
        var appConfig = new
        {
            app_name = appName,
            enable_subdomains = true,
            network = networkName,
            org_slug = "personal"
        };
        var flyCreationRequestContent = new StringContent(JsonSerializer.Serialize(appConfig),
            System.Text.Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync(
            "https://api.machines.dev/v1/apps", flyCreationRequestContent);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorResponseContent = await response.Content.ReadAsStringAsync();
            var errorCode = response.StatusCode;
            throw new Exception(
                $"Fly app creation failed with error code: {errorCode} and response content: {errorResponseContent}");
        }
    }
    
    private async Task StartMachine(string machineId, string appName)
    {
        HttpResponseMessage response;
        var content = new StringContent(JsonSerializer.Serialize(new { }), // Empty body as per Fly.io API
            System.Text.Encoding.UTF8, "application/json");

        var retryCounter = 0;
        while ((response=await _httpClient.PostAsync(
                   $"https://api.machines.dev/v1/apps/{appName}/machines/{machineId}/start", content))
               .IsSuccessStatusCode == false)
        {
            retryCounter++;

            var errorResponseContent = await response.Content.ReadAsStringAsync();
            var errorCode = response.StatusCode;
            Console.WriteLine($"Start machine request failed with error code: {errorCode} and response content: {errorResponseContent}. Retry counter: {retryCounter}");
            
            if (retryCounter == 2)
            {
                throw new Exception($"Failed to start machine at time: {DateTime.UtcNow}. Fly.io response status code: {response.StatusCode}. Machine id: '{machineId}'. App name: '{appName}'.");
            }
        }
    }

    private async Task WaitForMachineStarted(string appName, string machineId, string machineUrl, 
        int timeoutSeconds = 60)
    {
        
        var response = await _httpClient.GetAsync(
            $"https://api.machines.dev/v1/apps/{appName}/machines/{machineId}/wait?state=started&timeout={timeoutSeconds}");
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception(
                $"Failed to wait for machine started state. Status: {response.StatusCode}, Error: {errorContent}");
        }

        var healthCheckResponseTryCounter = 0;
        HttpResponseMessage? healthCheckResponse=null;
        string? healthCheckResponseErrorContent = null;
        while (healthCheckResponse?.IsSuccessStatusCode!=true)
        {
            if (healthCheckResponseTryCounter == 3)
            {
                throw new Exception(
                    $"Machine with id {machineId} failed its health check. Status: {healthCheckResponse?.StatusCode}, Error: {healthCheckResponseErrorContent}");
            }
            try
            {
                healthCheckResponse = await _httpClient.GetAsync(machineUrl + "/health");
                healthCheckResponseTryCounter++;
                healthCheckResponseErrorContent = await response.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                // 
            }
        }
    }
}