using System.Net.Http.Json;
using SalesForcePkce.Api.Models;
using SalesForcePkce.Api.Options;

namespace SalesForcePkce.Api.Services;

public sealed class SalesforceTokenClient(HttpClient httpClient) : ISalesforceTokenClient
{
    public async Task<SalesforceTokenResponse> ExchangeCodeForTokenAsync(SalesforceOptions options, string code, string codeVerifier, CancellationToken cancellationToken)
    {
        var formData = new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["client_id"] = options.ClientId,
            ["redirect_uri"] = options.RedirectUri,
            ["code"] = code,
            ["code_verifier"] = codeVerifier
        };

        using var response = await httpClient.PostAsync(options.TokenEndpoint, new FormUrlEncodedContent(formData), cancellationToken);
        response.EnsureSuccessStatusCode();

        var token = await response.Content.ReadFromJsonAsync<SalesforceTokenResponse>(cancellationToken: cancellationToken);
        return token ?? throw new InvalidOperationException("Salesforce token response was empty.");
    }
}
