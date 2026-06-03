using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using SalesForcePkce.Api.Models;
using SalesForcePkce.Api.Options;
using SalesForcePkce.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<SalesforceOptions>(builder.Configuration.GetSection(SalesforceOptions.SectionName));
builder.Services.Configure<McpOptions>(builder.Configuration.GetSection(McpOptions.SectionName));
builder.Services.Configure<FoundryOptions>(builder.Configuration.GetSection(FoundryOptions.SectionName));

builder.Services.AddSingleton<PkceService>();
builder.Services.AddSingleton<PkceStateStore>();
builder.Services.AddSingleton<SalesforceTokenStore>();

builder.Services.AddHttpClient<ISalesforceTokenClient, SalesforceTokenClient>();
builder.Services.AddScoped<IAgentChatService, AgentChatService>();

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/auth/salesforce/start", (
    IOptions<SalesforceOptions> options,
    PkceService pkceService,
    PkceStateStore stateStore) =>
{
    var pkceState = pkceService.CreateState();
    var codeVerifier = pkceService.GenerateCodeVerifier();
    var authorizationUrl = pkceService.BuildAuthorizationUrl(options.Value, pkceState, codeVerifier);

    stateStore.Set(pkceState, codeVerifier);

    return TypedResults.Ok(new SalesforceAuthStartResponse(authorizationUrl, pkceState));
});

app.MapGet("/api/auth/salesforce/callback", async Task<Results<Ok<SalesforceAuthCallbackResponse>, BadRequest<string>>>(
    string? code,
    string? state,
    IOptions<SalesforceOptions> options,
    PkceStateStore stateStore,
    SalesforceTokenStore tokenStore,
    ISalesforceTokenClient tokenClient,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(state))
    {
        return TypedResults.BadRequest("Missing OAuth code or state.");
    }

    if (!stateStore.TryConsume(state, out var codeVerifier))
    {
        return TypedResults.BadRequest("Invalid or expired OAuth state.");
    }

    var tokenResponse = await tokenClient.ExchangeCodeForTokenAsync(options.Value, code, codeVerifier, cancellationToken);
    tokenStore.Set(tokenResponse);

    return TypedResults.Ok(new SalesforceAuthCallbackResponse("Salesforce PKCE authentication completed."));
});

app.MapPost("/api/chat", async Task<Results<Ok<ChatResponse>, BadRequest<string>>>(
    ChatRequest request,
    SalesforceTokenStore tokenStore,
    IAgentChatService chatService,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.Message))
    {
        return TypedResults.BadRequest("Message is required.");
    }

    var token = tokenStore.GetAccessToken();

    try
    {
        var response = await chatService.SendAsync(request.Message, token, cancellationToken);
        return TypedResults.Ok(new ChatResponse(response));
    }
    catch (Exception ex)
    {
        return TypedResults.BadRequest($"Agent error: {ex.Message}");
    }
});

app.Run();
