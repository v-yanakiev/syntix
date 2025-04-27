using AbstractExecution;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Enums;

namespace LocalExecution;

public class LocalAgentSetup(PostgresContext postgresContext) : IExecutionSetup
{
    public async Task<AgentInfo> GetAgentInfoAsync(long executionMachineTemplateId, Guid? chatId=null,string userId="")
    {
        var executionMachineTemplate=await 
            postgresContext.FlyMachineTemplates.Where(a=>a.Id==executionMachineTemplateId).SingleAsync();
        
        var fakeMachine = new ExecutionMachine(); // in a local environment, the only "machine" we have is the devenvironment itself
        
        return new AgentInfo(executionMachineTemplate,fakeMachine,"http://localhost:65432");
    }

    public Task InitializeMachineAsync(long executionMachineTemplateId, Guid? chatId, string userId)
    {
        return Task.CompletedTask;
    }

    public Task StopMachinesAssociatedWithChatAsync(Guid chatId,CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }
}