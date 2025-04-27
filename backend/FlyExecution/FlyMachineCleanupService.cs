using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Models;

namespace FlyExecution;

public class FlyMachineCleanupService(
    IDbContextFactory<PostgresContext> contextFactory,
    IConfiguration configuration,
    HttpClient httpClient) : BackgroundService
{
    private readonly Random _random = new();
    private const int BaseCleanupIntervalMinutes = 35;
    private const int RandomOffsetMinutes = 5; // +/- 5 minutes
    private const int InactiveThresholdMinutes = 30;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", configuration["FLY_ACCESS_TOKEN"]!);
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        
        while (!stoppingToken.IsCancellationRequested)
        {
            await using var dbContext = await contextFactory.CreateDbContextAsync(stoppingToken);
            
            var inactiveCutoff = DateTime.UtcNow.AddMinutes(-InactiveThresholdMinutes);
            var inactiveYetRunningMachines = await dbContext.ExecutionMachine
                .Where(m => m.LastActive != null && m.LastActive < inactiveCutoff).ToListAsync(stoppingToken);
            await StopMachines(inactiveYetRunningMachines, httpClient, dbContext, stoppingToken);

            // Calculate next interval with random offset
            var randomOffset = _random.Next(-RandomOffsetMinutes, RandomOffsetMinutes + 1);
            var nextInterval = TimeSpan.FromMinutes(BaseCleanupIntervalMinutes + randomOffset);
            await Task.Delay(nextInterval, stoppingToken);
        }
    }

    public static async Task StopMachines(List<ExecutionMachine> inactiveYetRunningMachines, HttpClient httpClient,
        PostgresContext dbContext, CancellationToken stoppingToken = new())
    {
        foreach (var machine in inactiveYetRunningMachines)
        {
            try
            {
                await StopFlyMachine(machine, httpClient, stoppingToken);
                var logMessage = $"SUCCESS 4: Stopped machine at time {DateTime.UtcNow}, with id {machine.Id} and app name {machine.AppName}, requested by user {machine.UserId}.";

                machine.LastActive = null;
                machine.ChatId = null;
                machine.UserId = null;
                
                dbContext.Logs.Add(new Log() { Content =  logMessage});
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to stop machine {machine.Id}: {ex.Message}");
            }
        }

        await dbContext.SaveChangesAsync(stoppingToken);
    }

    private static async Task StopFlyMachine(ExecutionMachine machine, HttpClient httpClient,
        CancellationToken stoppingToken)
    {
        var response = await httpClient.PostAsync(
            $"https://api.machines.dev/v1/apps/{machine.AppName}/machines/{machine.Id}/stop",
            new StringContent("{}", System.Text.Encoding.UTF8, "application/json"), stoppingToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorResponseContent = await response.Content.ReadAsStringAsync(stoppingToken);
            var errorCode = response.StatusCode;
            throw new Exception(
                $"Fly machine stopping failed with error code: {errorCode} and response content: {errorResponseContent}");
        }
    }
}