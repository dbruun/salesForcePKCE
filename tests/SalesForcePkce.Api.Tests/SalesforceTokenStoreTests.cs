using SalesForcePkce.Api.Models;
using SalesForcePkce.Api.Services;

namespace SalesForcePkce.Api.Tests;

public class SalesforceTokenStoreTests
{
    [Fact]
    public void GetAccessToken_ReturnsTokenWhenNotExpired()
    {
        var fakeTime = new FakeTimeProvider(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
        var store = new SalesforceTokenStore(fakeTime);
        store.Set(new SalesforceTokenResponse { AccessToken = "token-1", ExpiresInSeconds = 60 });

        var token = store.GetAccessToken();

        Assert.Equal("token-1", token);
    }

    [Fact]
    public void GetAccessToken_ReturnsNullWhenExpired()
    {
        var fakeTime = new FakeTimeProvider(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
        var store = new SalesforceTokenStore(fakeTime);
        store.Set(new SalesforceTokenResponse { AccessToken = "token-2", ExpiresInSeconds = 1 });

        fakeTime.Advance(TimeSpan.FromSeconds(2));
        var token = store.GetAccessToken();

        Assert.Null(token);
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
