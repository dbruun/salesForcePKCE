using System.Text;
using System.Text.Json;

namespace SalesForcePkce.Api.Services;

public sealed class HttpMcpClient(HttpClient httpClient) : IMcpClient
{
    public async Task<string> CallToolAsync(string serverUrl, string toolName, object arguments, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(serverUrl))
        {
            throw new InvalidOperationException("MCP server URL is not configured.");
        }

        var requestPayload = new
        {
            jsonrpc = "2.0",
            id = Guid.NewGuid().ToString("N"),
            method = "tools/call",
            @params = new
            {
                name = toolName,
                arguments
            }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, serverUrl)
        {
            Content = new StringContent(JsonSerializer.Serialize(requestPayload), Encoding.UTF8, "application/json")
        };

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(payload))
        {
            return string.Empty;
        }

        using var jsonDoc = JsonDocument.Parse(payload);
        if (jsonDoc.RootElement.TryGetProperty("result", out var result))
        {
            return result.ToString();
        }

        return payload;
    }
}
