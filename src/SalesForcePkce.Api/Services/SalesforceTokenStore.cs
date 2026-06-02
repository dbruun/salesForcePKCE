using SalesForcePkce.Api.Models;

namespace SalesForcePkce.Api.Services;

public sealed class SalesforceTokenStore
{
    private readonly Lock _lock = new();
    private SalesforceTokenResponse? _token;

    public void Set(SalesforceTokenResponse token)
    {
        lock (_lock)
        {
            _token = token;
        }
    }

    public string? GetAccessToken()
    {
        lock (_lock)
        {
            return _token?.AccessToken;
        }
    }
}
