using System.Collections.Concurrent;

namespace SalesForcePkce.Api.Services;

public sealed class PkceStateStore
{
    private static readonly TimeSpan StateLifetime = TimeSpan.FromMinutes(10);
    private readonly TimeProvider _timeProvider;
    private readonly ConcurrentDictionary<string, PkceStateEntry> _stateToVerifier = new();

    public PkceStateStore()
        : this(TimeProvider.System)
    {
    }

    public PkceStateStore(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public void Set(string state, string codeVerifier)
    {
        _stateToVerifier[state] = new PkceStateEntry(codeVerifier, _timeProvider.GetUtcNow());
    }

    public bool TryConsume(string state, out string codeVerifier)
    {
        codeVerifier = string.Empty;
        if (!_stateToVerifier.TryRemove(state, out var entry))
        {
            return false;
        }

        if (_timeProvider.GetUtcNow() - entry.CreatedAt > StateLifetime)
        {
            return false;
        }

        codeVerifier = entry.CodeVerifier;
        return true;
    }

    private sealed record PkceStateEntry(string CodeVerifier, DateTimeOffset CreatedAt);
}
