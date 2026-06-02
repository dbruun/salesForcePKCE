using SalesForcePkce.Api.Services;

namespace SalesForcePkce.Api.Tests;

public class PkceStateStoreTests
{
    [Fact]
    public void TryConsume_ReturnsVerifierWhenStateIsFresh()
    {
        var fakeTime = new FakeTimeProvider(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
        var store = new PkceStateStore(fakeTime);

        store.Set("state-1", "verifier-1");

        var ok = store.TryConsume("state-1", out var verifier);

        Assert.True(ok);
        Assert.Equal("verifier-1", verifier);
    }

    [Fact]
    public void TryConsume_ReturnsFalseWhenStateIsExpired()
    {
        var fakeTime = new FakeTimeProvider(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
        var store = new PkceStateStore(fakeTime);

        store.Set("state-2", "verifier-2");
        fakeTime.Advance(TimeSpan.FromMinutes(11));

        var ok = store.TryConsume("state-2", out var verifier);

        Assert.False(ok);
        Assert.Equal(string.Empty, verifier);
    }

    private sealed class FakeTimeProvider(DateTimeOffset now) : TimeProvider
    {
        private DateTimeOffset _now = now;

        public override DateTimeOffset GetUtcNow() => _now;

        public void Advance(TimeSpan duration)
        {
            _now = _now.Add(duration);
        }
    }
}
