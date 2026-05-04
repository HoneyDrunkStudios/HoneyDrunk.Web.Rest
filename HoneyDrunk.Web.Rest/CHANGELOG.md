# HoneyDrunk.Web.Rest - Repository Changelog

All notable changes to the HoneyDrunk.Web.Rest repository will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

**Note:** See individual package CHANGELOGs for detailed changes:
- [HoneyDrunk.Web.Rest.Abstractions CHANGELOG](HoneyDrunk.Web.Rest.Abstractions/CHANGELOG.md)
- [HoneyDrunk.Web.Rest.AspNetCore CHANGELOG](HoneyDrunk.Web.Rest.AspNetCore/CHANGELOG.md)

---

## [0.4.0] - 2026-05-04

### Changed

- Updated `HoneyDrunk.Kernel.Abstractions` from 0.4.0 to 0.5.0 for typed `TenantId` context adoption.
- Updated `HoneyDrunk.Transport` from 0.4.0 to 0.5.0.
- `RequestLoggingScopeMiddleware` now omits the `TenantId` log-scope property for Internal-tenant requests and emits the sanitized ULID string for non-Internal requests.

## [0.3.0] - 2026-04-25

### Added

- Added ADR-0005/0006 Web.Rest bootstrap support for env-var-driven Azure Key Vault and Azure App Configuration through `AddWebRestBootstrap()`.
- Added `/internal/vault/invalidate` endpoint mapping via `MapHoneyDrunkWebRestVaultInvalidationWebhook()` for Event Grid cache invalidation.
- Added canary coverage for Key Vault `SecretNewVersionCreated` invalidation using `{Provider}--{Key}` secret names.

### Changed

- Updated `HoneyDrunk.Web.Rest.AspNetCore` to consume the Vault Azure Key Vault, App Configuration, and Event Grid provider packages.

## [0.2.0] - 2026-02-14

### Added

- Exception mapping for Kernel typed exceptions (`ValidationException` → 400, `NotFoundException` → 404, `ConcurrencyException` → 409, `SecurityException` → 403, `DependencyFailureException` → 503)
- `JsonException` and `BadHttpRequestException` mapping to 400 Bad Request
- `ExceptionMappingMiddleware` guards against `Response.HasStarted`
- `CorrelationMiddleware` logs warning when Kernel and header correlation IDs differ
- `RestAuthorizationResultHandler` falls back to `HttpContext.User.Identity.IsAuthenticated`
- `PolicyNotFound` authorization deny code

### Changed

- Updated HoneyDrunk.Kernel.Abstractions from 0.3.0 to 0.4.0
- Updated HoneyDrunk.Transport from 0.3.0 to 0.4.0
- Updated HoneyDrunk.Auth.AspNetCore from 0.1.0 to 0.2.0

## [0.1.0] - 2026-01-10

### Added

- Initial release of HoneyDrunk.Web.Rest
- `ApiResult` / `ApiResult<T>` result envelopes with standard status codes
- `ApiError`, `ApiErrorCode`, `ApiErrorResponse`, `ValidationError` error types
- `PageRequest` / `PageResult<T>` pagination primitives
- Standard header names and media type constants
- Telemetry tag constants for observability
- ASP.NET Core integration: `CorrelationMiddleware`, `ExceptionMappingMiddleware`, `RequestLoggingMiddleware`
- `RestOptions` for configurable middleware behavior
- Fluent `AddHoneyDrunkWebRest()` service registration with Kernel and Auth integration

[0.4.0]: https://github.com/HoneyDrunkStudios/HoneyDrunk.Web.Rest/releases/tag/v0.4.0
[0.3.0]: https://github.com/HoneyDrunkStudios/HoneyDrunk.Web.Rest/releases/tag/v0.3.0
[0.2.0]: https://github.com/HoneyDrunkStudios/HoneyDrunk.Web.Rest/releases/tag/v0.2.0
[0.1.0]: https://github.com/HoneyDrunkStudios/HoneyDrunk.Web.Rest/releases/tag/v0.1.0
