using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SalesForcePkce.Api.Services;

public sealed class HttpMcpClient(HttpClient httpClient, McpSessionStore sessionStore) : IMcpClient
{
    private bool _initialized;

    public async Task<string> CallToolAsync(string serverUrl, string accessToken, string toolName, object arguments, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(serverUrl))
        {
            throw new InvalidOperationException("MCP server URL is not configured.");
        }

        if (!_initialized)
        {
            await InitializeAsync(serverUrl, accessToken, cancellationToken);
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

        var payload = await SendMcpRequestAsync(serverUrl, accessToken, requestPayload, cancellationToken);
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

    private async Task InitializeAsync(string serverUrl, string accessToken, CancellationToken cancellationToken)
    {
        var initPayload = new
        {
            jsonrpc = "2.0",
            id = Guid.NewGuid().ToString("N"),
            method = "initialize",
            @params = new
            {
                protocolVersion = "2025-03-26",
                capabilities = new { },
                clientInfo = new
                {
                    name = "SalesForcePkce.Api",
                    version = "1.0.0"
                }
            }
        };

        await SendMcpRequestAsync(serverUrl, accessToken, initPayload, cancellationToken);

        var notificationPayload = new
        {
            jsonrpc = "2.0",
            method = "notifications/initialized"
        };

        await SendMcpRequestAsync(serverUrl, accessToken, notificationPayload, cancellationToken);

        _initialized = true;
    }

    private async Task<string> SendMcpRequestAsync(string serverUrl, string accessToken, object payload, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, serverUrl)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

        var sessionId = sessionStore.SessionId;
        if (sessionId is not null)
        {
            request.Headers.Add("Mcp-Session-Id", sessionId);
        }

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        if (response.Headers.TryGetValues("Mcp-Session-Id", out var sessionValues))
        {
            sessionStore.SessionId = sessionValues.FirstOrDefault();
        }

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}
