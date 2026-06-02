using System.Collections.Concurrent;

namespace SalesForcePkce.Api.Services;

public sealed class PkceStateStore
{
    private readonly ConcurrentDictionary<string, string> _stateToVerifier = new();

    public void Set(string state, string codeVerifier)
    {
        _stateToVerifier[state] = codeVerifier;
    }

    public bool TryConsume(string state, out string codeVerifier)
    {
        return _stateToVerifier.TryRemove(state, out codeVerifier!);
    }
}
