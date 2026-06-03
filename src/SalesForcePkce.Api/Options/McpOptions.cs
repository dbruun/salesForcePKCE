namespace SalesForcePkce.Api.Options;

public sealed class McpOptions
{
    public const string SectionName = "Mcp";

    public string ServerUrl { get; set; } = "https://api.salesforce.com/platform/mcp/v1/sandbox/platform/sobject-all";
    public string ChatToolName { get; set; } = "salesforce.chat";
}
