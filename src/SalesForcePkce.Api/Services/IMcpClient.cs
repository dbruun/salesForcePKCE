namespace SalesForcePkce.Api.Services;

public interface IMcpClient
{
    Task<string> CallToolAsync(string serverUrl, string accessToken, string toolName, object arguments, CancellationToken cancellationToken);
}
