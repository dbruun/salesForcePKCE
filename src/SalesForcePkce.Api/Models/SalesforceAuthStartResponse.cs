namespace SalesForcePkce.Api.Models;

public sealed record SalesforceAuthStartResponse(string AuthorizationUrl, string State);
