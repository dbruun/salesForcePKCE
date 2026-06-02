using Azure.AI.Extensions.OpenAI;
using Azure.AI.Projects;
using Azure.AI.Projects.Agents;
using Azure.Identity;
using Microsoft.Extensions.Options;
using OpenAI.Responses;
using SalesForcePkce.Api.Options;

namespace SalesForcePkce.Api.Services;

#pragma warning disable OPENAI001
public sealed class FoundryAgentClient(IOptions<FoundryOptions> options) : IFoundryAgentClient
{
    public async Task<string> BuildAgentPromptAsync(string message, CancellationToken cancellationToken)
    {
        var settings = options.Value;

        if (string.IsNullOrWhiteSpace(settings.Endpoint) ||
            string.IsNullOrWhiteSpace(settings.AgentName) ||
            string.IsNullOrWhiteSpace(settings.AgentVersion))
        {
            return message;
        }

        try
        {
            var projectClient = new AIProjectClient(
                endpoint: new Uri(settings.Endpoint),
                tokenProvider: new DefaultAzureCredential());

            AgentReference agentReference = new(name: settings.AgentName, version: settings.AgentVersion);
            ProjectResponsesClient responseClient = projectClient.OpenAI.GetProjectResponsesClientForAgent(agentReference);

            ResponseResult response = await responseClient.CreateResponseAsync(message, cancellationToken: cancellationToken);
            var output = response.GetOutputText();

            return string.IsNullOrWhiteSpace(output) ? message : output;
        }
        catch
        {
            return message;
        }
    }
}
#pragma warning restore OPENAI001
