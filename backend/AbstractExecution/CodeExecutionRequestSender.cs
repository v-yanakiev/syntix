using System.Text;
using System.Text.Json;

namespace AbstractExecution;

public static class CodeExecutionRequestSender
{
    public static Task<HttpResponseMessage> SendAsync(string url, CodeExecutionParameters parameters,
        string ephemeralKey,
        HttpClient client)
    {
        var toSendToMachineServer = new
        {
            environment = parameters.Environment,
            code = parameters.Code,
            dependencies = parameters.Dependencies,
            ephemeralKey
        };

        return client.PostAsync(url,
            new StringContent(ToMachineParameterSerializer.Serialize(toSendToMachineServer), Encoding.UTF8,
                "application/json"));
    }
}