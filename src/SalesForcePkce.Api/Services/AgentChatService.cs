using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Client;
using SalesForcePkce.Api.Options;

namespace SalesForcePkce.Api.Services;

public sealed class AgentChatService(
    IOptions<McpOptions> mcpOptions,
    IOptions<FoundryOptions> foundryOptions) : IAgentChatService
{
    public async Task<string> SendAsync(string message, string salesforceAccessToken, CancellationToken cancellationToken)
    {
        var mcp = mcpOptions.Value;
        var foundry = foundryOptions.Value;

        // Connect to Salesforce MCP server with the user's OAuth token
        await using var mcpClient = await McpClient.CreateAsync(
            new HttpClientTransport(new HttpClientTransportOptions
            {
                Endpoint = new Uri(mcp.ServerUrl),
                AdditionalHeaders = new Dictionary<string, string>
                {
                    ["Authorization"] = $"Bearer {salesforceAccessToken}"
                }
            }), cancellationToken: cancellationToken);

        // Discover available MCP tools from the Salesforce server
        var mcpTools = await mcpClient.ListToolsAsync(cancellationToken: cancellationToken);

        // Create an agent with MCP tools — the framework handles all tool orchestration
        var agent = new AIProjectClient(new Uri(foundry.Endpoint), new DefaultAzureCredential())
            .AsAIAgent(
                model: foundry.ModelDeployment,
                instructions: foundry.Instructions,
                tools: [.. mcpTools.Cast<AITool>()]);

        // The agent autonomously decides which tools to call and synthesizes the response
        var response = await agent.RunAsync(message, cancellationToken: cancellationToken);
        return response.ToString();
    }
}
