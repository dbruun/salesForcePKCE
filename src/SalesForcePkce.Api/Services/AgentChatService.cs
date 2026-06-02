using Microsoft.Extensions.Options;
using SalesForcePkce.Api.Options;

namespace SalesForcePkce.Api.Services;

public sealed class AgentChatService(
    IMcpClient mcpClient,
    IFoundryAgentClient foundryAgentClient,
    IOptions<McpOptions> mcpOptions) : IAgentChatService
{
    public async Task<string> SendAsync(string message, string salesforceAccessToken, CancellationToken cancellationToken)
    {
        var effectivePrompt = await foundryAgentClient.BuildAgentPromptAsync(message, cancellationToken);
        var options = mcpOptions.Value;

        return await mcpClient.CallToolAsync(
            options.ServerUrl,
            options.ChatToolName,
            new
            {
                message = effectivePrompt,
                accessToken = salesforceAccessToken
            },
            cancellationToken);
    }
}
