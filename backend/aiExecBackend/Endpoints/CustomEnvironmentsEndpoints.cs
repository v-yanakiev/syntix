using AbstractExecution;
using aiExecBackend.Endpoints.Common;
using aiExecBackend.Extensions;
using FlyExecution;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Enums;
using UserEnvironmentBuilder;

namespace aiExecBackend.Endpoints;

public static class CustomEnvironmentsEndpoints
{
    private static async Task<long> GetEnvironmentCreatingTemplateId(PostgresContext postgresContext)
    {
        var environmentCreatingTemplate =
            await postgresContext.FlyMachineTemplates.FirstAsync(a =>
                a.Type == CodeExecutionEnvironment.EnvironmentDefining.ToString());
        return environmentCreatingTemplate.Id;
    }
    public static async Task<IResult> StartEnvironmentBuilderHandler(SignInManager<UserInfo> signInManager,
        PostgresContext postgresContext,
        IExecutionSetup executionSetup)
    {
        var user = await signInManager.GetUserWithExecutedExpression();
        if (user == null) return Results.Unauthorized();

        var executionEnvironmentCreatingTemplateId = await GetEnvironmentCreatingTemplateId(postgresContext);
        
        await executionSetup.InitializeMachineAsync(executionEnvironmentCreatingTemplateId, null, user.Id);
        return Results.Ok();
    }

    public static async Task UploadHandler(HttpContext context,PostgresContext postgresContext, SignInManager<UserInfo> signInManager,
        IExecutionSetup executionSetup, IRequestForwarder forwarder)
    {
        var executionEnvironmentCreatingTemplateId = await GetEnvironmentCreatingTemplateId(postgresContext);

        await Forwarder.ForwardRequest(executionEnvironmentCreatingTemplateId, null, context, signInManager,
            executionSetup, forwarder, "/saveFiles");
    }

    public static async Task ScanDirectory(HttpContext context,PostgresContext postgresContext, SignInManager<UserInfo> signInManager,
        IExecutionSetup executionSetup, IRequestForwarder forwarder)
    {        
        var executionEnvironmentCreatingTemplateId = await GetEnvironmentCreatingTemplateId(postgresContext);

        await Forwarder.ForwardRequest(executionEnvironmentCreatingTemplateId, null, context, signInManager,
            executionSetup, forwarder, "/scanDirectory");
    }

    public static async Task<IResult> BuildEnvironmentHandler(BuildEnvironmentRequestData requestData,
        PostgresContext postgresContext,
        UserEnvironmentBuilder.Builder userEnvironmentBuilder, SignInManager<UserInfo> signInManager)
    {
        var user = await signInManager.GetUserWithExecutedExpression();
        if (user == null) return Results.Unauthorized();
        
        var executionEnvironmentCreatingTemplateId = await GetEnvironmentCreatingTemplateId(postgresContext);

        try
        {
            await userEnvironmentBuilder.BuildEnvironment(executionEnvironmentCreatingTemplateId, user.Id, requestData);
            return Results.Ok();
        }
        catch (Exception ex)
        {
            var unescapedMessage = System.Text.RegularExpressions.Regex.Unescape(ex.Message);
            return Results.InternalServerError(unescapedMessage);
        }
    }

    public static async Task<IResult> GetAllCustomEnvironments(SignInManager<UserInfo> signInManager)
    {
        var user = await signInManager.GetUserWithExecutedExpression(a =>
            a.Include(b => b.CustomExecutionMachineTemplates));
        if (user == null) return Results.Unauthorized();
        
        var environmentsInfo = user.CustomExecutionMachineTemplates.Select(a =>
            new CustomEnvironmentInfo(a.Id, a.Name, a.CodeFile!, a.AfterChangesValidationCommand!,a.DependencyInstallingTerminalCall!,
                a.RootDirectory!,
                a.ProgrammingLanguage!,
                a.ImageUrl == null ));
        
        return Results.Ok(environmentsInfo);
    }

    public static async Task<IResult> EditCustomEnvironment(CustomEnvironmentInfo customEnvironmentInfo,
        SignInManager<UserInfo> signInManager, PostgresContext postgresContext)
    {
        var user = await signInManager.GetUserWithExecutedExpression(a => a.Include(b => b.CustomExecutionMachineTemplates));
        if (user == null) return Results.Unauthorized();
        
        var customEnvironment =
            user.CustomExecutionMachineTemplates.FirstOrDefault(a => a.Id == customEnvironmentInfo.Id);
        if (customEnvironment == null) return Results.NotFound();
        
        customEnvironment.Name = customEnvironmentInfo.Name;
        customEnvironment.CodeFile = customEnvironmentInfo.CodeFile;
        customEnvironment.AfterChangesValidationCommand = customEnvironmentInfo.AfterChangesValidationCommand;
        customEnvironment.DependencyInstallingTerminalCall = customEnvironmentInfo.DependencyInstallingTerminalCall;
        customEnvironment.RootDirectory= customEnvironmentInfo.RootDirectory;
        customEnvironment.ProgrammingLanguage= customEnvironmentInfo.ProgrammingLanguage;
        
        await postgresContext.SaveChangesAsync();
        return Results.Ok();
    }

    public static async Task<IResult> DeleteCustomEnvironment(long customEnvironmentId,
        SignInManager<UserInfo> signInManager, PostgresContext postgresContext, FlyAppCleanupService flyAppCleanupService)
    {
        var user = await signInManager.GetUserWithExecutedExpression(a =>
            a.Include(b => b.CustomExecutionMachineTemplates).
                ThenInclude(b=>b.ExecutionMachines));
        
        if (user == null) return Results.Unauthorized();
        
        var customEnvironment = user.CustomExecutionMachineTemplates.FirstOrDefault(a => a.Id == customEnvironmentId);
        if (customEnvironment == null) return Results.NotFound();
        
        if (customEnvironment.AppName != null)
        {
            await flyAppCleanupService.SendFlyAppDeleteRequestAsync(customEnvironment.AppName);
        }
        
        //will be only 1 for the foreseeable future
        var machinesGeneratedWithTheTemplate = customEnvironment.ExecutionMachines; 
        foreach(var machine in machinesGeneratedWithTheTemplate)
        {
            await flyAppCleanupService.SendFlyAppDeleteRequestAsync(machine.AppName);
        }
        
        postgresContext.FlyMachineTemplates.Remove(customEnvironment);
        await postgresContext.SaveChangesAsync();
        return Results.Ok();
    }
}

public record CustomEnvironmentInfo(
    long Id,
    string Name,
    string CodeFile,
    string AfterChangesValidationCommand,
    string DependencyInstallingTerminalCall,
    string RootDirectory,
    string ProgrammingLanguage,
    bool? BuildInProgress);