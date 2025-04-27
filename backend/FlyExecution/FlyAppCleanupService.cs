using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;

namespace FlyExecution;

public class FlyAppCleanupService(
    IConfiguration configuration,
    HttpClient httpClient)
{
    public async Task SendFlyAppDeleteRequestAsync(string appName)
    {
        await SendFlyAppDeleteRequestAsyncExplicitHttpClient(appName, httpClient,configuration["FLY_ACCESS_TOKEN"]!);
    }
    public static async Task SendFlyAppDeleteRequestAsyncExplicitHttpClient(string appName, HttpClient explicitHttpClient, string flyKey)
    {
        explicitHttpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", flyKey);
        
        explicitHttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Delete,
            RequestUri = new Uri($"https://api.machines.dev/v1/apps/{appName}"),
        };
        
        using var response = await explicitHttpClient.SendAsync(request);
        
        if (!response.IsSuccessStatusCode&&response.StatusCode!=HttpStatusCode.NotFound) // we don't care about duplicate deletion attempts
        {
            var errorResponseContent = await response.Content.ReadAsStringAsync();
            var errorCode = response.StatusCode;
            throw new Exception(
                $"Fly app deletion failed with error code: {errorCode} and response content: {errorResponseContent}");
        }
    }
}