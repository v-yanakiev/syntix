using AbstractExecution;
using aiExecBackend.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Enums;

namespace aiExecBackend.Endpoints.Common;

public static class Forwarder
{
    public static async Task ForwardRequest(long executionEnvironmentTemplateId, Guid? chatId, HttpContext context, 
        SignInManager<UserInfo> signInManager, IExecutionSetup executionSetup,
        IRequestForwarder forwarder, string agentEndpoint)
    {
        var user = await signInManager.GetUserWithExecutedExpression(a => a.Include(b => b.Chats));
        if (user == null) return;
        
        var agentUrl = (await executionSetup.GetAgentInfoAsync(executionEnvironmentTemplateId, chatId, user.Id)).Url;
        await forwarder.ForwardAsync(context, agentUrl + agentEndpoint);
    }
}