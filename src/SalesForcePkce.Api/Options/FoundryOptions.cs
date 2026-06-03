namespace SalesForcePkce.Api.Options;

public sealed class FoundryOptions
{
    public const string SectionName = "Foundry";

    public string Endpoint { get; set; } = string.Empty;
    public string ModelDeployment { get; set; } = "gpt-4o-mini";
    public string Instructions { get; set; } = "You are a helpful Salesforce assistant. Use the available tools to query and manage Salesforce data on behalf of the authenticated user.";
}
