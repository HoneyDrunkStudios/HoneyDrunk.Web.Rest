# HoneyDrunk.Web.Rest

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/download/dotnet/10.0)

> **Standardized REST API conventions for HoneyDrunk.OS** - Unified response envelopes, correlation propagation, exception mapping, and API contracts for consistent REST services across the Hive.

**Signal Quote:** *"Every response speaks the same language."*

---

## What Is This?

HoneyDrunk.Web.Rest provides **REST API conventions and middleware** for HoneyDrunk.OS ("the Hive"). It ensures consistent API responses, error handling, and observability across all Nodes in the Grid:

- **Response Envelopes** - Unified `ApiResult<T>` and `ApiErrorResponse` contracts
- **Correlation Propagation** - `X-Correlation-Id` header handling with `ICorrelationIdAccessor`
- **Exception Mapping** - Automatic exception-to-HTTP-status mapping with configurable strategies
- **Model Validation** - Consistent validation error responses with `ValidationError` collections
- **Pagination Contracts** - `PageRequest` and `PageResult<T>` for standardized paging
- **JSON Conventions** - camelCase, enum-as-string, null-omission defaults
- **Observability** - Request logging scopes and telemetry tag constants
- **Kernel Integration** - Prefers `IOperationContext.CorrelationId` when available
- **Auth Integration** - Shapes 401/403 as `ApiErrorResponse` using `IAuthenticatedIdentityAccessor`
- **Transport Integration** - Maps `ITransportEnvelope` metadata to `ApiResult` for HTTP responses (no transport runtime)

---

## Quick Start

### Installation

```xml
<ItemGroup>
  <!-- Full ASP.NET Core integration -->
  <PackageReference Include="HoneyDrunk.Web.Rest.AspNetCore" Version="0.2.0" />
  
  <!-- Or just the contracts (no runtime dependencies) -->
  <PackageReference Include="HoneyDrunk.Web.Rest.Abstractions" Version="0.2.0" />
</ItemGroup>
```

### Configure in Program.cs

```csharp
using HoneyDrunk.Web.Rest.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 1. Register REST services
builder.Services.AddRest(options =>
{
    options.EnableExceptionMapping = true;
    options.EnableRequestLoggingScope = true;
    options.EnableModelStateValidationFilter = true;
    options.IncludeExceptionDetails = builder.Environment.IsDevelopment();
    options.IncludeTraceId = true;
});

builder.Services.AddControllers();

var app = builder.Build();

// 2. Add REST middleware (early in pipeline, before endpoints)
app.UseRest();

app.MapControllers();

app.Run();
```

---

## Kernel/Auth/Transport Integration

The middleware automatically integrates with other HoneyDrunk packages when they are registered:

### Kernel Integration

When `IOperationContextAccessor` is registered (from `HoneyDrunk.Kernel`):

```csharp
// Correlation ID priority:
// 1. IOperationContext.CorrelationId (Kernel)
// 2. X-Correlation-Id header
// 3. Generated GUID

// Logging scope is enriched with:
// - OperationId, OperationName, CausationId
// - TenantId, ProjectId
// - NodeId, StudioId, Environment (from IGridContext)
```

### Auth Integration

When `IAuthenticatedIdentityAccessor` is registered (from `HoneyDrunk.Auth.AspNetCore`):

```csharp
// 401/403 responses are shaped as ApiErrorResponse
// Error messages are context-aware based on authentication state
```

### Transport Integration

Map transport envelopes to API results (mapping helpers only, no transport runtime):

```csharp
using HoneyDrunk.Web.Rest.AspNetCore.Transport;

// From ITransportEnvelope
ApiResult result = envelope.ToApiResult();
ApiResult<Order> result = envelope.ToApiResult(orderData);

// From operation outcomes
ApiResult success = TransportResultExtensions.ToSuccessResult(correlationId);
ApiResult<Order> successWithData = TransportResultExtensions.ToSuccessResult(orderData, correlationId);
ApiResult failure = TransportResultExtensions.ToFailureResult("Operation failed", correlationId);

// Conditional success/failure based on outcome
ApiResult outcome = TransportResultExtensions.FromOutcome(
    isSuccess: true,
    errorMessage: null,
    correlationId: correlationId);
```

---

## Usage Examples

### Returning Successful Responses

```csharp
using HoneyDrunk.Web.Rest.Abstractions.Results;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    [HttpGet("{id}")]
    public IActionResult GetOrder(Guid id)
    {
        var order = _orderService.GetById(id);
        
        if (order is null)
        {
            return NotFound(ApiResult.NotFound("Order not found."));
        }
        
        return Ok(ApiResult<Order>.Success(order));
    }
    
    [HttpPost]
    public IActionResult CreateOrder([FromBody] CreateOrderRequest request)
    {
        var order = _orderService.Create(request);
        return Ok(ApiResult<Order>.Success(order));
    }
}
```

### Response Shapes

**Success Response:**
```json
{
  "status": "success",
  "correlationId": "abc-123",
  "traceId": "00-abc123...",
  "timestamp": "2026-01-10T00:00:00Z",
  "data": {
    "id": "order-123",
    "total": 99.99
  }
}
```

**Error Response:**
```json
{
  "correlationId": "abc-123",
  "traceId": "00-abc123...",
  "timestamp": "2026-01-10T00:00:00Z",
  "error": {
    "code": "NOT_FOUND",
    "message": "Order not found."
  }
}
```

### Minimal API Support

```csharp
using HoneyDrunk.Web.Rest.AspNetCore.MinimalApi;

app.MapGet("/orders/{id}", (Guid id, IOrderService orderService) =>
{
    var order = orderService.GetById(id);
    return order is not null
        ? Results.Ok(ApiResult<Order>.Success(order))
        : Results.NotFound(ApiResult.NotFound("Order not found."));
})
.WithRest<Order>();
```

---

## Configuration Options

```csharp
services.AddRest(options =>
{
    // Middleware toggles
    options.EnableExceptionMapping = true;          // Exception-to-error mapping
    options.EnableRequestLoggingScope = true;       // Logging scope enrichment
    options.EnableModelStateValidationFilter = true; // MVC validation filter
    options.EnableAuthFailureShaping = true;        // Shape 401/403 as ApiErrorResponse
    
    // Response enrichment
    options.IncludeExceptionDetails = false;        // Stack traces in errors (dev only)
    options.IncludeTraceId = true;                  // OpenTelemetry trace ID
    
    // Correlation ID
    options.CorrelationIdHeaderName = "X-Correlation-Id";
    options.GenerateCorrelationIdIfMissing = true;
    options.ReturnCorrelationIdInResponseHeader = true;
    
    // JSON
    options.ConfigureJsonDefaults = true;           // Apply JSON conventions
});
```

---

## Project Structure

```
HoneyDrunk.Web.Rest/
+-- HoneyDrunk.Web.Rest.Abstractions/    # Pure contracts (no dependencies)
|   +-- Constants/                        # HeaderNames, MediaTypes
|   +-- Errors/                           # ApiError, ApiErrorCode, ValidationError
|   +-- Paging/                           # PageRequest, PageResult<T>
|   +-- Results/                          # ApiResult, ApiResult<T>, ApiResultStatus
|   +-- Telemetry/                        # RestTelemetryTags
|
+-- HoneyDrunk.Web.Rest.AspNetCore/      # ASP.NET Core integration
|   +-- Auth/                             # Authentication failure handling
|   +-- Configuration/                    # RestOptions
|   +-- Context/                          # ICorrelationIdAccessor
|   +-- Errors/                           # Exception mapping
|   +-- Extensions/                       # DI and middleware extensions
|   +-- Middleware/                       # Correlation, exception, logging
|   +-- MinimalApi/                       # Endpoint conventions
|   +-- Mvc/                              # Filters and conventions
|   +-- Serialization/                    # JSON defaults
|   +-- Transport/                        # Transport result extensions
|
+-- HoneyDrunk.Web.Rest.Tests/           # Integration tests
|   +-- Abstractions/                     # Contract tests
|   +-- AspNetCore/                       # Middleware tests
|   +-- TestHost/                         # Test infrastructure
|
+-- HoneyDrunk.Web.Rest.Canary/          # Canary integration tests
    +-- 26 tests across 5 invariants
    +-- Validates upstream contract compatibility
```

---

## Related Projects

| Project | Relationship |
|---------|--------------|
| **[HoneyDrunk.Kernel](https://github.com/HoneyDrunkStudios/HoneyDrunk.Kernel)** | Core abstractions and Grid context (optional integration) |
| **[HoneyDrunk.Auth](https://github.com/HoneyDrunkStudios/HoneyDrunk.Auth)** | Authentication and authorization (optional integration) |
| **[HoneyDrunk.Transport](https://github.com/HoneyDrunkStudios/HoneyDrunk.Transport)** | Messaging infrastructure (optional integration) |
| **[HoneyDrunk.Data](https://github.com/HoneyDrunkStudios/HoneyDrunk.Data)** | Persistence conventions |
| **[HoneyDrunk.Standards](https://github.com/HoneyDrunkStudios/HoneyDrunk.Standards)** | Analyzers and coding conventions |

---

## License

This project is licensed under the [MIT License](LICENSE).

---

<div align="center">

**Built with honey by HoneyDrunk Studios**

[GitHub](https://github.com/HoneyDrunkStudios/HoneyDrunk.Web.Rest) | [Issues](https://github.com/HoneyDrunkStudios/HoneyDrunk.Web.Rest/issues)

</div>