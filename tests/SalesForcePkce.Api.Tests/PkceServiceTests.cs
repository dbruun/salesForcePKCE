using SalesForcePkce.Api.Options;
using SalesForcePkce.Api.Services;

namespace SalesForcePkce.Api.Tests;

public class PkceServiceTests
{
    [Fact]
    public void CreateCodeChallenge_UsesExpectedRfcVector()
    {
        const string verifier = "dBjftJeZ4CVP-mB92K27uhbUJU1p1r_wW1gFWFOEjXk";

        var challenge = PkceService.CreateCodeChallenge(verifier);

        Assert.Equal("E9Melhoa2OwvFrEMTJguCHaoeK1t8URWbuGJSstw-cM", challenge);
    }

    [Fact]
    public void BuildAuthorizationUrl_IncludesPkceAndOAuthParameters()
    {
        var service = new PkceService();
        var options = new SalesforceOptions
        {
            AuthorizationEndpoint = "https://login.salesforce.com/services/oauth2/authorize",
            ClientId = "client-id-123",
            RedirectUri = "http://localhost:5000/api/auth/salesforce/callback",
            Scope = "api refresh_token"
        };

        const string state = "state123";
        const string verifier = "verifier123";

        var url = service.BuildAuthorizationUrl(options, state, verifier);

        Assert.Contains("response_type=code", url);
        Assert.Contains("client_id=client-id-123", url);
        Assert.Contains("state=state123", url);
        Assert.Contains("code_challenge_method=S256", url);
        Assert.Contains("code_challenge=", url);
    }
}
