namespace SalesForcePkce.Api.Options;

public sealed class FoundryOptions
{
    public const string SectionName = "Foundry";

    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string AgentId { get; set; } = string.Empty;
}
