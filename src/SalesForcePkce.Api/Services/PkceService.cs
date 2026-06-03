using System.Security.Cryptography;
using System.Text;
using SalesForcePkce.Api.Options;

namespace SalesForcePkce.Api.Services;

public sealed class PkceService
{
    public string CreateState() => Base64UrlEncode(RandomNumberGenerator.GetBytes(32));

    public string GenerateCodeVerifier() => Base64UrlEncode(RandomNumberGenerator.GetBytes(64));

    public string BuildAuthorizationUrl(SalesforceOptions options, string state, string codeVerifier)
    {
        var codeChallenge = CreateCodeChallenge(codeVerifier);

        var query = new Dictionary<string, string>
        {
            ["response_type"] = "code",
            ["client_id"] = options.ClientId,
            ["redirect_uri"] = options.RedirectUri,
            ["scope"] = options.Scope,
            ["state"] = state,
            ["code_challenge"] = codeChallenge,
            ["code_challenge_method"] = "S256"
        };

        var queryString = string.Join("&", query.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
        return $"{options.AuthorizationEndpoint}?{queryString}";
    }

    public static string CreateCodeChallenge(string codeVerifier)
    {
        var bytes = SHA256.HashData(Encoding.ASCII.GetBytes(codeVerifier));
        return Base64UrlEncode(bytes);
    }

    private static string Base64UrlEncode(byte[] input)
    {
        return Convert.ToBase64String(input)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }
}
