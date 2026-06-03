using Azure.AI.Extensions.OpenAI;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Client;
using SalesForcePkce.Api.Options;

namespace SalesForcePkce.Api.Services;

public sealed class AgentChatService(
    IOptions<McpOptions> mcpOptions,
    IOptions<FoundryOptions> foundryOptions,
    ILogger<AgentChatService> logger) : IAgentChatService
{
    public async Task<string> SendAsync(string message, string? salesforceAccessToken, CancellationToken cancellationToken)
    {
        var mcp = mcpOptions.Value;
        var foundry = foundryOptions.Value;

        var tools = new List<AITool>();
        McpClient? mcpClient = null;

        try
        {
            // If authenticated with Salesforce, connect to the MCP server and discover tools
            if (!string.IsNullOrWhiteSpace(salesforceAccessToken) && !string.IsNullOrWhiteSpace(mcp.ServerUrl))
            {
                mcpClient = await McpClient.CreateAsync(
                    new HttpClientTransport(new HttpClientTransportOptions
                    {
                        Endpoint = new Uri(mcp.ServerUrl),
                        AdditionalHeaders = new Dictionary<string, string>
                        {
                            ["Authorization"] = $"Bearer {salesforceAccessToken}"
                        }
                    }), cancellationToken: cancellationToken);

                var mcpTools = await mcpClient.ListToolsAsync(cancellationToken: cancellationToken);
                tools.AddRange(mcpTools.Cast<AITool>());
            }

            // Connect to the existing Foundry agent, with MCP tools if available
            var credential = string.IsNullOrWhiteSpace(foundry.TenantId)
                ? new DefaultAzureCredential()
                : new DefaultAzureCredential(new DefaultAzureCredentialOptions { TenantId = foundry.TenantId });

            var projectClient = new AIProjectClient(new Uri(foundry.Endpoint), credential);
            var agentRef = new AgentReference(name: foundry.AgentName, version: foundry.AgentVersion);

            var agent = projectClient.AsAIAgent(agentRef, tools: tools);

            var response = await agent.RunAsync(message, cancellationToken: cancellationToken);

            logger.LogInformation("Agent response - Text: '{Text}', Messages: {Count}, FinishReason: {Reason}",
                response.Text, response.Messages?.Count, response.FinishReason);

            if (response.Messages is { Count: > 0 })
            {
                foreach (var msg in response.Messages)
                {
                    logger.LogInformation("Message role={Role}, text='{Text}', contents={Count}",
                        msg.Role, msg.Text, msg.Contents?.Count);
                    if (msg.Contents is not null)
                    {
                        foreach (var content in msg.Contents)
                        {
                            logger.LogInformation("  Content type={Type}, raw={Raw}",
                                content.GetType().FullName,
                                content.RawRepresentation?.GetType().FullName);
                        }
                    }
                }
            }

            // Try Text first
            if (!string.IsNullOrWhiteSpace(response.Text))
            {
                return response.Text;
            }

            // Fall back to extracting text from the last assistant message
            var lastAssistant = response.Messages?.LastOrDefault(m => m.Role == Microsoft.Extensions.AI.ChatRole.Assistant);
            if (lastAssistant is not null)
            {
                if (!string.IsNullOrWhiteSpace(lastAssistant.Text))
                {
                    return lastAssistant.Text;
                }

                // Extract text from content parts
                var textParts = new List<string>();
                if (lastAssistant.Contents is not null)
                {
                    foreach (var content in lastAssistant.Contents)
                    {
                        if (content is Microsoft.Extensions.AI.TextContent tc && !string.IsNullOrWhiteSpace(tc.Text))
                        {
                            textParts.Add(tc.Text);
                        }
                        else if (content.RawRepresentation is not null)
                        {
                            // Try to extract text from the raw OpenAI response object
                            var raw = content.RawRepresentation;
                            var textProp = raw.GetType().GetProperty("Text");
                            if (textProp?.GetValue(raw) is string rawText && !string.IsNullOrWhiteSpace(rawText))
                            {
                                textParts.Add(rawText);
                            }
                        }
                    }
                }

                if (textParts.Count > 0)
                {
                    return string.Join("", textParts);
                }
            }

            return string.Empty;
        }
        finally
        {
            if (mcpClient is not null)
            {
                await mcpClient.DisposeAsync();
            }
        }
    }
}
