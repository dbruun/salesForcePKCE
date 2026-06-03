namespace SalesForcePkce.Api.Services;

public sealed class McpSessionStore
{
    private readonly object _lock = new();
    private string? _sessionId;

    public string? SessionId
    {
        get { lock (_lock) { return _sessionId; } }
        set { lock (_lock) { _sessionId = value; } }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _sessionId = null;
        }
    }
}
