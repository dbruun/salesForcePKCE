using SalesForcePkce.Api.Models;

namespace SalesForcePkce.Api.Services;

public sealed class SalesforceTokenStore
{
    private readonly object _lock = new();
    private readonly TimeProvider _timeProvider;
    private SalesforceTokenResponse? _token;
    private DateTimeOffset? _expiresAt;

    public SalesforceTokenStore()
        : this(TimeProvider.System)
    {
    }

    public SalesforceTokenStore(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public void Set(SalesforceTokenResponse token)
    {
        lock (_lock)
        {
            _token = token;
            _expiresAt = token.ExpiresInSeconds is > 0
                ? _timeProvider.GetUtcNow().AddSeconds(token.ExpiresInSeconds.Value)
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

            if (_expiresAt is not null && _timeProvider.GetUtcNow() >= _expiresAt.Value)
            {
                _token = null;
                _expiresAt = null;
                return null;
            }

            return _token?.AccessToken;
        }
    }
}
