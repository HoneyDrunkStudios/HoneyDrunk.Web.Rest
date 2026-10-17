# HoneyDrunk.Web.Rest.AspNetCore

Middleware, filters, and endpoint helpers that enforce HoneyDrunk REST conventions for ASP.NET Core services.

## Quick Start

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add Kernel first so Web.Rest can require a live request context
builder.Services.AddHoneyDrunkNode(options =>
{
    options.NodeId = new("my-node");
    // Configure the remaining node identity options here.
});

// Add REST services
builder.Services.AddRest(options =>
{
    options.IncludeExceptionDetails = builder.Environment.IsDevelopment();
    options.EnableRequestLoggingScope = true;
});

var app = builder.Build();

// Establish Kernel request context before REST middleware
app.UseGridContext();
app.UseRest();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
```

For deployable HoneyDrunk nodes, use the ADR-0005/0006 bootstrap helper with `AZURE_KEYVAULT_URI`,
`AZURE_APPCONFIG_ENDPOINT`, and `HONEYDRUNK_NODE_ID` supplied by the host environment:

```csharp
builder.Services.AddHoneyDrunkNode(options =>
{
    options.NodeId = new("honeydrunk-web-rest");
    // Configure the remaining node identity options here.
})
.AddWebRestBootstrap();

var app = builder.Build();
app.MapHoneyDrunkWebRestVaultInvalidationWebhook();
```

## Features

### ADR-0005/0006 Bootstrap

- Reads Key Vault location from `AZURE_KEYVAULT_URI`
- Reads shared App Configuration location from `AZURE_APPCONFIG_ENDPOINT`
- Uses `HONEYDRUNK_NODE_ID` as the App Configuration label, for example `honeydrunk-web-rest`
- Registers the Event Grid `/internal/vault/invalidate` webhook for Vault cache invalidation
- Keeps application secrets behind `ISecretStore`; no Web.Rest source reads secret values from `IConfiguration`

### Correlation ID Propagation

- Requires Kernel `IOperationContext.CorrelationId` from a live request context
- Returns the Kernel correlation ID in response headers
- Makes the Kernel correlation ID available via `ICorrelationIdAccessor`
- Logs a warning when incoming `X-Correlation-Id` differs from Kernel context

### Kernel Integration

Web.Rest requires `HoneyDrunk.Kernel` request context:
- Call `AddHoneyDrunkNode()` before `AddRest()`
- Call `UseGridContext()` before `UseRest()`
- Uses `IOperationContextAccessor.Current.CorrelationId` as the correlation source
- Enriches logging scopes with Kernel context values:
  - `OperationId`, `OperationName`, `CausationId`
  - `TenantId`, `ProjectId`
  - `NodeId`, `StudioId`, `Environment` (from `IGridContext`)
- Emits `TenantId` only for non-Internal requests; Internal-tenant traffic is represented by the absence of the property.

### Auth Integration

When `HoneyDrunk.Auth.AspNetCore` is registered:
- Uses `IAuthenticatedIdentityAccessor` to provide context-aware error messages
- Shapes 401/403 responses as `ApiErrorResponse` via `IAuthorizationMiddlewareResultHandler`
- Falls back to `HttpContext.User.Identity.IsAuthenticated` when `IAuthenticatedIdentityAccessor` is not registered

### Transport Integration

Extension methods for mapping `ITransportEnvelope` to `ApiResult`:
```csharp
// From envelope
ApiResult result = envelope.ToApiResult();
ApiResult<T> result = envelope.ToApiResult(data);

// From outcome
ApiResult result = TransportResultExtensions.FromOutcome(isSuccess, errorMessage, correlationId);
```

### Exception Mapping

All unhandled exceptions are mapped to `ApiErrorResponse`:

| Exception Type | HTTP Status | Error Code |
|----------------|-------------|------------|
| JsonException | 400 | BAD_REQUEST |
| BadHttpRequestException | 400 | BAD_REQUEST |
| ArgumentException | 400 | BAD_REQUEST |
| Kernel ValidationException | 400 | BAD_REQUEST |
| InvalidOperationException | 409 | CONFLICT |
| Kernel ConcurrencyException | 409 | CONFLICT |
| KeyNotFoundException | 404 | NOT_FOUND |
| Kernel NotFoundException | 404 | NOT_FOUND |
| UnauthorizedAccessException | 403 | FORBIDDEN |
| Kernel SecurityException | 403 | FORBIDDEN |
| Kernel DependencyFailureException | 503 | SERVICE_UNAVAILABLE |
| NotImplementedException | 501 | NOT_IMPLEMENTED |
| OperationCanceledException | 499 | GENERAL_ERROR |
| Other | 500 | INTERNAL_ERROR |

> **Note:** Kernel exceptions use safe static messages. The `HasStarted` guard prevents errors when response streaming has begun.

### Model Validation

Invalid model state is automatically converted to `ApiErrorResponse` with validation errors:

```json
{
  "correlationId": "abc-123",
  "timestamp": "2026-01-10T00:00:00Z",
  "error": {
    "code": "VALIDATION_FAILED",
    "message": "One or more validation errors occurred."
  },
  "validationErrors": [
    { "field": "email", "message": "Email is required." }
  ]
}
```

### Minimal API Support

```csharp
app.MapGet("/api/items/{id}", GetItemAsync)
   .WithRest<ItemDto>();

app.MapPost("/api/items", CreateItemAsync)
   .WithRestCreate<ItemDto>();

app.MapDelete("/api/items/{id}", DeleteItemAsync)
   .WithRestDelete();
```

### Configuration Options

| Option | Default | Description |
|--------|---------|-------------|
| CorrelationIdHeaderName | X-Correlation-Id | Header name for correlation ID |
| IncludeExceptionDetails | false | Include exception details in errors |
| IncludeTraceId | true | Include trace ID in responses |
| ReturnCorrelationIdInResponseHeader | true | Return correlation ID in headers |
| GenerateCorrelationIdIfMissing | true | Legacy option; Web.Rest correlation now comes from Kernel request context |
| EnableRequestLoggingScope | true | Enable logging scope middleware |
| EnableExceptionMapping | true | Enable exception mapping middleware |
| EnableModelStateValidationFilter | true | Enable model validation filter |
| ConfigureJsonDefaults | true | Configure JSON serialization |
| EnableAuthFailureShaping | true | Shape 401/403 as ApiErrorResponse |

## Dependencies

- HoneyDrunk.Web.Rest.Abstractions (contracts)
- HoneyDrunk.Kernel.Abstractions 0.7.0 (required, for request context and typed exceptions)
- HoneyDrunk.Auth.AspNetCore 0.4.0 (optional, for identity context)
- HoneyDrunk.Transport 0.6.0 (optional, for envelope mapping)
- HoneyDrunk.Vault.EventGrid 0.5.0 (for cache invalidation webhook mapping)
- HoneyDrunk.Vault.Providers.AppConfiguration 0.5.0 (for env-var-driven App Configuration bootstrap)
- HoneyDrunk.Vault.Providers.AzureKeyVault 0.5.0 (for env-var-driven Key Vault bootstrap)
- Microsoft.AspNetCore.App (framework reference)
