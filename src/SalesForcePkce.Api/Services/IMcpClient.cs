using System.Text.Json;

namespace SalesForcePkce.Api.Services;

public interface IMcpClient
{
    Task<string> CallToolAsync(string serverUrl, string toolName, object arguments, CancellationToken cancellationToken);
}
