namespace SalesForcePkce.Api.Options;

public sealed class FoundryOptions
{
    public const string SectionName = "Foundry";

    public string Endpoint { get; set; } = string.Empty;
    public string AgentName { get; set; } = string.Empty;
    public string AgentVersion { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
}
