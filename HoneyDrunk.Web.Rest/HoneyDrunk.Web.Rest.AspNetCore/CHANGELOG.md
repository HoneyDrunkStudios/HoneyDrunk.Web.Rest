# Changelog

All notable changes to HoneyDrunk.Web.Rest.AspNetCore will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.2.0] - 2026-02-14

### Changed

#### Dependencies
- Bumped `HoneyDrunk.Kernel.Abstractions` from 0.3.0 to 0.4.0
- Bumped `HoneyDrunk.Transport` from 0.3.0 to 0.4.0
- Bumped `HoneyDrunk.Auth.AspNetCore` from 0.1.0 to 0.2.0

### Added

#### Exception Mapping
- `JsonException` maps to 400 Bad Request (`BAD_REQUEST`) — malformed JSON bodies
- `BadHttpRequestException` maps to 400 Bad Request (`BAD_REQUEST`) — malformed requests
- Kernel `ValidationException` maps to 400 Bad Request (`BAD_REQUEST`)
- Kernel `NotFoundException` maps to 404 Not Found (`NOT_FOUND`)
- Kernel `ConcurrencyException` maps to 409 Conflict (`CONFLICT`)
- Kernel `SecurityException` maps to 403 Forbidden (`FORBIDDEN`)
- Kernel `DependencyFailureException` maps to 503 Service Unavailable (`SERVICE_UNAVAILABLE`)
- All Kernel exception messages use safe static strings — no internal message leakage

#### Middleware
- `ExceptionMappingMiddleware` now guards against `Response.HasStarted` — exception is still logged but no response is attempted when headers have already been sent
- `CorrelationMiddleware` now accepts `ILogger<CorrelationMiddleware>` and logs a warning when both Kernel and header correlation IDs are present but differ (includes header value, kernel value, HTTP method, and request path)

#### Auth
- `RestAuthorizationResultHandler.WriteForbiddenResponseAsync` falls back to `HttpContext.User.Identity.IsAuthenticated` when `IAuthenticatedIdentityAccessor` is not registered

### Notes
- Kernel typed exceptions are matched before BCL fallbacks in the switch expression to ensure specificity
- The HasStarted guard prevents `InvalidOperationException` from being thrown when middleware runs after streaming has begun
- Correlation mismatch warning helps diagnose middleware ordering issues in multi-context environments

## [0.1.0] - 2026-01-10

### Added

#### Configuration
- `RestOptions` - Central configuration for all REST middleware and filters
  - `CorrelationIdHeaderName` - Configurable header name (default: X-Correlation-Id)
  - `IncludeExceptionDetails` - Stack traces in errors (default: false)
  - `IncludeTraceId` - OpenTelemetry trace ID in responses (default: true)
  - `ReturnCorrelationIdInResponseHeader` - Echo correlation ID back (default: true)
  - `GenerateCorrelationIdIfMissing` - Auto-generate if not provided (default: true)
  - `EnableRequestLoggingScope` - Logging scope enrichment (default: true)
  - `EnableExceptionMapping` - Exception-to-error mapping (default: true)
  - `EnableModelStateValidationFilter` - MVC validation filter (default: true)
  - `ConfigureJsonDefaults` - Apply JSON conventions (default: true)
  - `EnableAuthFailureShaping` - Shape 401/403 as ApiErrorResponse (default: true)

#### Middleware
- `CorrelationMiddleware` - Extracts/generates correlation ID with priority:
  1. Kernel `IOperationContext.CorrelationId` (if `IOperationContextAccessor` is registered)
  2. Incoming `X-Correlation-Id` header
  3. Generated GUID
- `ExceptionMappingMiddleware` - Catches unhandled exceptions, maps to `ApiErrorResponse` with appropriate HTTP status codes
- `RequestLoggingScopeMiddleware` - Enriches logging scope with:
  - Correlation ID, HTTP method, path, request ID, trace ID
  - Kernel context (if available): OperationId, OperationName, CausationId, TenantId, ProjectId, NodeId, StudioId, Environment

#### Kernel Integration
- Uses `HoneyDrunk.Kernel.Abstractions.Context.IOperationContextAccessor` when registered
- Prefers `IOperationContext.CorrelationId` over request headers
- Enriches logging scope with `IGridContext` values (NodeId, StudioId, Environment)

#### Auth Integration
- Uses `HoneyDrunk.Auth.AspNetCore.IAuthenticatedIdentityAccessor` when registered
- `RestAuthorizationResultHandler` - Implements `IAuthorizationMiddlewareResultHandler` to shape 401/403 responses as `ApiErrorResponse`
- Registered automatically by `AddRest()` when `EnableAuthFailureShaping` is true
- `RestAuthExtensions` - Helper methods for writing auth error responses:
  - `WriteUnauthorizedResponseAsync()` - Writes 401 response as `ApiErrorResponse`
  - `WriteForbiddenResponseAsync()` - Writes 403 response as `ApiErrorResponse`

#### Transport Integration
- `TransportResultExtensions` - Extension methods for `HoneyDrunk.Transport.Abstractions.ITransportEnvelope`:
  - `envelope.ToApiResult()` - Creates ApiResult with envelope's CorrelationId
  - `envelope.ToApiResult(data)` - Creates ApiResult<T> with data and CorrelationId
  - `TransportResultExtensions.FromOutcome(...)` - Creates ApiResult from success/failure outcome

#### Exception Mapping
- `ExceptionToApiErrorMapper` - Maps common exceptions to HTTP status codes:
  - `ArgumentNullException` -> 400 Bad Request
  - `ArgumentException` -> 400 Bad Request
  - `InvalidOperationException` -> 409 Conflict
  - `KeyNotFoundException` -> 404 Not Found
  - `UnauthorizedAccessException` -> 403 Forbidden
  - `NotImplementedException` -> 501 Not Implemented
  - `OperationCanceledException` -> 499 Client Closed
  - Other exceptions -> 500 Internal Server Error

#### MVC Support
- `ModelStateValidationFilter` - Converts invalid model state to `ApiErrorResponse` with validation errors

#### Minimal API Support
- `RestEndpointConventions` - Extension methods for minimal API endpoints:
  - `WithRest()` - Standard REST conventions
  - `WithRest<TResponse>()` - Typed success response
  - `WithRestCreate<TResponse>()` - Create operations (201 Created)
  - `WithRestDelete()` - Delete operations (204 No Content)
  - `RequireRestAuth()` - Authorization with error shaping

#### Context
- `ICorrelationIdAccessor` - Contract for accessing current correlation ID
- `CorrelationIdAccessor` - AsyncLocal implementation that flows across async boundaries

#### Serialization
- `JsonOptionsDefaults` - Default JSON serializer options:
  - camelCase property naming
  - Case-insensitive property matching
  - Null properties omitted
  - Enums serialized as camelCase strings
  - Compact output (not indented)

#### Extensions
- `ServiceCollectionExtensions.AddRest()` - DI registration
- `ApplicationBuilderExtensions.UseRest()` - Middleware pipeline registration

### Notes
- Middleware is registered in correct order: Correlation -> ExceptionMapping -> LoggingScope
- Model state validation filter suppresses default ASP.NET Core validation behavior
- JSON defaults are automatically applied to MVC JsonOptions
- All middleware respects configuration toggles
- Kernel/Auth/Transport integrations are optional - middleware gracefully degrades when not registered
- Auth failure shaping uses `IAuthorizationMiddlewareResultHandler` for scheme-agnostic 401/403 handling

[0.2.0]: https://github.com/HoneyDrunkStudios/HoneyDrunk.Web.Rest/releases/tag/v0.2.0
[0.1.0]: https://github.com/HoneyDrunkStudios/HoneyDrunk.Web.Rest/releases/tag/v0.1.0
