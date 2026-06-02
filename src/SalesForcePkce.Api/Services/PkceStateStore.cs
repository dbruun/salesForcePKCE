using System.Collections.Concurrent;

namespace SalesForcePkce.Api.Services;

public sealed class PkceStateStore
{
    private static readonly TimeSpan StateLifetime = TimeSpan.FromMinutes(10);
    private readonly ConcurrentDictionary<string, PkceStateEntry> _stateToVerifier = new();

    public void Set(string state, string codeVerifier)
    {
        _stateToVerifier[state] = new PkceStateEntry(codeVerifier, DateTimeOffset.UtcNow);
    }

    public bool TryConsume(string state, out string codeVerifier)
    {
        codeVerifier = string.Empty;
        if (!_stateToVerifier.TryRemove(state, out var entry))
        {
            return false;
        }

        if (DateTimeOffset.UtcNow - entry.CreatedAt > StateLifetime)
        {
            return false;
        }

        codeVerifier = entry.CodeVerifier;
        return true;
    }

    private sealed record PkceStateEntry(string CodeVerifier, DateTimeOffset CreatedAt);
}
