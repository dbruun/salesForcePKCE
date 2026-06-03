using System.Text.Json.Serialization;

namespace SalesForcePkce.Api.Models;

public sealed class SalesforceTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }

    [JsonPropertyName("token_type")]
    public string? TokenType { get; set; }

    [JsonPropertyName("instance_url")]
    public string? InstanceUrl { get; set; }

    [JsonPropertyName("expires_in")]
    public int? ExpiresInSeconds { get; set; }
}
