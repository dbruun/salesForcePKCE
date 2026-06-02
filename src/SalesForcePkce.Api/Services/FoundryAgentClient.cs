using Azure.AI.Extensions.OpenAI;
using Azure.AI.Projects;
using Azure.AI.Projects.Agents;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Responses;
using SalesForcePkce.Api.Options;

namespace SalesForcePkce.Api.Services;

public sealed class FoundryAgentClient : IFoundryAgentClient
{
    private readonly ILogger<FoundryAgentClient> _logger;
    private readonly ProjectResponsesClient? _responseClient;

    public FoundryAgentClient(IOptions<FoundryOptions> options, ILogger<FoundryAgentClient> logger)
    {
        _logger = logger;
        var settings = options.Value;

        if (string.IsNullOrWhiteSpace(settings.Endpoint) ||
            string.IsNullOrWhiteSpace(settings.AgentName) ||
            string.IsNullOrWhiteSpace(settings.AgentVersion))
        {
            return;
        }

        var projectClient = new AIProjectClient(
            endpoint: new Uri(settings.Endpoint),
            tokenProvider: new DefaultAzureCredential());

        AgentReference agentReference = new(name: settings.AgentName, version: settings.AgentVersion);
        _responseClient = projectClient.OpenAI.GetProjectResponsesClientForAgent(agentReference);
    }

    public async Task<string> BuildAgentPromptAsync(string message, CancellationToken cancellationToken)
    {
        if (_responseClient is null)
        {
            return message;
        }

        try
        {
            // OPENAI001 is required by the Azure AI Projects/OpenAI Responses preview API surface.
#pragma warning disable OPENAI001
            ResponseResult response = await _responseClient.CreateResponseAsync(message, cancellationToken: cancellationToken);
#pragma warning restore OPENAI001
            var output = response.GetOutputText();

            return string.IsNullOrWhiteSpace(output) ? message : output;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Foundry agent prompt shaping failed. Falling back to original message.");
            return message;
        }
    }
}
