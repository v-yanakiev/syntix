using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Models;
using Models.Enums;
using Models.Migrations;

namespace AbstractExecution;

public class CodeExecutor(HttpClient httpClient, IExecutionSetup executionSetup)
{
    public async IAsyncEnumerable<string> ExecuteCodeAsync(CodeExecutionParameters parameters, long executionMachineTemplateId,
        Guid chatId = new (), string userId = "")
    {
        switch (parameters.Environment)
        {
            case CodeExecutionEnvironment.NodeTS:
            case CodeExecutionEnvironment.NodeJS:
            case CodeExecutionEnvironment.CSharp:
            case CodeExecutionEnvironment.Java:
            case CodeExecutionEnvironment.Python:
            case CodeExecutionEnvironment.Go:
            case CodeExecutionEnvironment.Rust:
            case CodeExecutionEnvironment.PostgreSQL:
            case CodeExecutionEnvironment.Custom:
                await foreach (var message in StreamCodeExecution(parameters,executionMachineTemplateId, chatId, userId))
                    yield return message;
                break;
            default:
                throw new JsonException($"Language {parameters.Environment} not supported at the moment!");
        }
    }

    private async IAsyncEnumerable<string> StreamCodeExecution(CodeExecutionParameters parameters,
        long executionMachineTemplateId,
        Guid chatId, string userId)
    {
        var agentInfo = await executionSetup.GetAgentInfoAsync(executionMachineTemplateId, chatId, userId);
        var url = agentInfo.Url + "/execute";
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
        
        
        var content = new
        {
            environment = parameters.Environment.ToString(),
            code = parameters.Code,
            dependencies = parameters.Dependencies,
            codeFile=agentInfo.Template.CodeFile,
            afterChangesValidationCommand=agentInfo.Template.AfterChangesValidationCommand,
            dependencyInstallingTerminalCall=agentInfo.Template.DependencyInstallingTerminalCall,
            doNotCreateNewFile = parameters.DoNotCreateNewFile
        };
        
        var serializedContent = JsonSerializer.Serialize(content);
        request.Content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        HttpResponseMessage response;
        try
        {
            response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            if (!response.IsSuccessStatusCode)
            {
                var errorResponseContent = await response.Content.ReadAsStringAsync();
                var errorCode = response.StatusCode;
                throw new Exception(
                    $"Code execution request returned error code: {errorCode} and response content: {errorResponseContent}");
            }
        }
        catch (Exception e)
        {
            throw new Exception($"Code execution request failed with error: \"{e.Message}\"");
        }
        
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);
        while (await reader.ReadLineAsync() is { } line)
        {
            string text;
            if (string.IsNullOrWhiteSpace(line))
            {
                yield return string.Empty;
                continue;
            }

            try
            {
                var deserialized = ServerSentEvent.Parse(line);
                text = deserialized != null ? deserialized.Data.Text : string.Empty;
            }
            catch (Exception)
            {
                text = string.Empty;
            }

            yield return text;
        }
    }
}