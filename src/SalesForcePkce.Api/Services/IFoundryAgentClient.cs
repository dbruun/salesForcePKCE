namespace SalesForcePkce.Api.Services;

public interface IFoundryAgentClient
{
    Task<string> BuildAgentPromptAsync(string message, CancellationToken cancellationToken);
}
