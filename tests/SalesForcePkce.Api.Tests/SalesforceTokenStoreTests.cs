using SalesForcePkce.Api.Models;
using SalesForcePkce.Api.Services;

namespace SalesForcePkce.Api.Tests;

public class SalesforceTokenStoreTests
{
    [Fact]
    public void GetAccessToken_ReturnsTokenWhenNotExpired()
    {
        var store = new SalesforceTokenStore();
        store.Set(new SalesforceTokenResponse { AccessToken = "token-1", ExpiresInSeconds = 60 });

        var token = store.GetAccessToken();

        Assert.Equal("token-1", token);
    }

    [Fact]
    public void GetAccessToken_ReturnsNullWhenExpired()
    {
        var store = new SalesforceTokenStore();
        store.Set(new SalesforceTokenResponse { AccessToken = "token-2", ExpiresInSeconds = 1 });

        Thread.Sleep(TimeSpan.FromSeconds(2));
        var token = store.GetAccessToken();

        Assert.Null(token);
    }
}
