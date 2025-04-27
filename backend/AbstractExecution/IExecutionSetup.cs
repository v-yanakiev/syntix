using Models;
using Models.Enums;

namespace AbstractExecution;

public interface IExecutionSetup
{
    Task InitializeMachineAsync(long executionMachineTemplateId, Guid? chatId, string userId);
    Task<AgentInfo> GetAgentInfoAsync(long executionMachineTemplateId, Guid? chatId, string userId); 
    Task StopMachinesAssociatedWithChatAsync(Guid chatId,CancellationToken stoppingToken);
}
public record AgentInfo(ExecutionMachineTemplate Template, ExecutionMachine Machine, string Url);