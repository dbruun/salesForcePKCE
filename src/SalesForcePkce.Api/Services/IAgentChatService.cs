namespace SalesForcePkce.Api.Services;

public interface IAgentChatService
{
    Task<string> SendAsync(string message, string salesforceAccessToken, CancellationToken cancellationToken);
}
