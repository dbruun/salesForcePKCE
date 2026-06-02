using SalesForcePkce.Api.Models;

namespace SalesForcePkce.Api.Services;

public sealed class SalesforceTokenStore
{
    private readonly object _lock = new();
    private SalesforceTokenResponse? _token;
    private DateTimeOffset? _expiresAt;

    public void Set(SalesforceTokenResponse token)
    {
        lock (_lock)
        {
            _token = token;
            _expiresAt = token.ExpiresInSeconds is > 0
                ? DateTimeOffset.UtcNow.AddSeconds(token.ExpiresInSeconds.Value)
                : null;
        }
    }

    public string? GetAccessToken()
    {
        lock (_lock)
        {
            if (_token is null)
            {
                return null;
            }

            if (_expiresAt is not null && DateTimeOffset.UtcNow >= _expiresAt.Value)
            {
                _token = null;
                _expiresAt = null;
                return null;
            }

            return _token?.AccessToken;
        }
    }
}
