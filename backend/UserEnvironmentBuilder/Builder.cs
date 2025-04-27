using System.Text;
using System.Text.Json;
using AbstractExecution;
using FlyExecution;
using Microsoft.AspNetCore.Identity;
using Models;
using Models.Enums;
using Tooling;

namespace UserEnvironmentBuilder;

public class Builder(
    PostgresContext postgresContext,
    HttpClient httpClient,
    IExecutionSetup executionSetup,
    FlyKeyTool flyKeyTool,
    FlyAppCleanupService flyAppCleanupService,
    FlyMachineSetup flyMachineSetup)
{
    public async Task BuildEnvironment(long environmentCreatingTemplateId, string userId, BuildEnvironmentRequestData buildEnvironmentRequestData)
    {
        
        var executionMachineTemplateToCreate = new ExecutionMachineTemplate()
        {
            Name = buildEnvironmentRequestData.EnvironmentName,
            AfterChangesValidationCommand =buildEnvironmentRequestData.ValidationCommand,
            CodeFile = buildEnvironmentRequestData.CodeFilePath,
            DependencyInstallingTerminalCall = buildEnvironmentRequestData.DependencyInstallingTerminalCall,
            RootDirectory = buildEnvironmentRequestData.RootDirectory,
            ProgrammingLanguage = buildEnvironmentRequestData.ProgrammingLanguage,
            Type = CodeExecutionEnvironment.Custom.ToString(), 
            CreatorId = userId,
        };
        postgresContext.FlyMachineTemplates.Add(executionMachineTemplateToCreate);
        await postgresContext.SaveChangesAsync();

        var templateAppName = "u" + Guid.NewGuid().ToString().Substring(1);
        AgentInfo? environmentBuilderAgentInfo = null;
        ExecutionMachine? singleExecutionMachine = null;
        
        try
        {
            await FlyMachineSetup.CreateFlyAppAsync(httpClient, templateAppName.Replace("-",""), templateAppName);
            executionMachineTemplateToCreate.AppName = templateAppName;

            var flyKey = await flyKeyTool.GetAppScopedKey(templateAppName);

            environmentBuilderAgentInfo =
                await executionSetup.GetAgentInfoAsync(environmentCreatingTemplateId, null, userId);

            var urlToTemplateBuilderApp = environmentBuilderAgentInfo.Url;
            var requestContent = new { appName = templateAppName, appScopedFlyKey = flyKey };
            
            using var request = new HttpRequestMessage(HttpMethod.Post, urlToTemplateBuilderApp + "/buildEnvironment");
            var serializedContent = JsonSerializer.Serialize(requestContent);
            
            request.Content = new StringContent(serializedContent, Encoding.UTF8, "application/json");
            using var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorResponseContent = await response.Content.ReadAsStringAsync();
                var errorCode = response.StatusCode;
                throw new Exception(
                    $"Environment builder request failed with error code: {errorCode} and response content: {errorResponseContent}");
            }
            
            var responseContent = await response.Content.ReadAsStringAsync();

            var buildResponse =
                JsonSerializer.Deserialize<BuildEnvironmentResponse>(responseContent, JsonSerializerOptions.Web);

            if (buildResponse?.ImageUrl == null)
            {
                throw new Exception("Build response could not be deserialized");
            }

            executionMachineTemplateToCreate.ImageUrl = buildResponse.ImageUrl;

            await postgresContext.SaveChangesAsync();
            singleExecutionMachine = await flyMachineSetup.CreateMachineAsync(executionMachineTemplateToCreate);

        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to build environment with userId={userId}, due to the following error: \"{e.Message}\", with the following stackTrace: {e.StackTrace}");
            if (executionMachineTemplateToCreate.AppName != null)
            {
                await flyAppCleanupService.SendFlyAppDeleteRequestAsync(executionMachineTemplateToCreate.AppName);
            }

            if (singleExecutionMachine != null)
            {
                await flyAppCleanupService.SendFlyAppDeleteRequestAsync(singleExecutionMachine.AppName);
            }

            postgresContext.FlyMachineTemplates.Remove(executionMachineTemplateToCreate);
            await postgresContext.SaveChangesAsync();

            throw;
        }
        finally
        {
            if (environmentBuilderAgentInfo != null)
            {
                await FlyMachineCleanupService.StopMachines([environmentBuilderAgentInfo.Machine],httpClient,postgresContext);
            }
        }
        
    }
}

public record BuildEnvironmentRequestData(string EnvironmentName, string ValidationCommand, string CodeFilePath, string DependencyInstallingTerminalCall, string RootDirectory, string ProgrammingLanguage);

public record BuildEnvironmentResponse(string ImageUrl);