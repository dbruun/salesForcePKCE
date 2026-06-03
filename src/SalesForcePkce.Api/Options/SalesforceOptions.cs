namespace SalesForcePkce.Api.Options;

public sealed class SalesforceOptions
{
    public const string SectionName = "Salesforce";

    public string AuthorizationEndpoint { get; set; } = "https://test.salesforce.com/services/oauth2/authorize";
    public string TokenEndpoint { get; set; } = "https://test.salesforce.com/services/oauth2/token";
    public string ClientId { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = "http://localhost:5000/api/auth/salesforce/callback";
    public string Scope { get; set; } = "mcp_api refresh_token";
}
