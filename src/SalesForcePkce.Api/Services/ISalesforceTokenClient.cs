using SalesForcePkce.Api.Models;
using SalesForcePkce.Api.Options;

namespace SalesForcePkce.Api.Services;

public interface ISalesforceTokenClient
{
    Task<SalesforceTokenResponse> ExchangeCodeForTokenAsync(SalesforceOptions options, string code, string codeVerifier, CancellationToken cancellationToken);
}
