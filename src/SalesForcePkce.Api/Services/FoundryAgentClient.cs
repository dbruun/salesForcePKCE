using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SalesForcePkce.Api.Options;

namespace SalesForcePkce.Api.Services;

public sealed class FoundryAgentClient(HttpClient httpClient, IOptions<FoundryOptions> options) : IFoundryAgentClient
{
    public async Task<string> BuildAgentPromptAsync(string message, CancellationToken cancellationToken)
    {
        var settings = options.Value;

        if (string.IsNullOrWhiteSpace(settings.Endpoint) || string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            return message;
        }

        var requestBody = new
        {
            agent_id = settings.AgentId,
            input = message
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, settings.Endpoint)
        {
            Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiKey);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return message;
        }

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(responseContent))
        {
            return message;
        }

        using var jsonDoc = JsonDocument.Parse(responseContent);
        if (jsonDoc.RootElement.TryGetProperty("output_text", out var outputText) && outputText.ValueKind == JsonValueKind.String)
        {
            return outputText.GetString() ?? message;
        }

        return message;
    }
}
