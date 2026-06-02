# salesForcePKCE

Minimal .NET + web chat app to connect an agent workflow to Salesforce through an MCP server using a PKCE OAuth flow.

## What this includes

- **.NET backend** with Salesforce PKCE endpoints:
  - `GET /api/auth/salesforce/start` to generate state + code challenge and start OAuth
  - `GET /api/auth/salesforce/callback` to exchange authorization code for token
- **Agent chat endpoint**:
  - `POST /api/chat` forwards chat through Foundry agent prompt shaping (optional) and then to the configured MCP server tool
- **Simple web chat UI** served from `wwwroot/index.html` ("react or whatever" option)

## Configuration

Set values in `src/SalesForcePkce.Api/appsettings.json`:

- `Salesforce.ClientId`
- `Salesforce.RedirectUri`
- `Mcp.ServerUrl`
- `Mcp.ChatToolName` (tool exposed by your MCP server)
- `Foundry.Endpoint`, `Foundry.ApiKey`, `Foundry.AgentId` (optional)

## Run

```bash
dotnet run --project src/SalesForcePkce.Api/SalesForcePkce.Api.csproj
```
