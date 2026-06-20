# Changelog

All notable changes to the HoneyDrunk.Web.Rest repository are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

For per-release and per-package detail, see:

- [Solution changelog](HoneyDrunk.Web.Rest/CHANGELOG.md)
- [HoneyDrunk.Web.Rest.Abstractions changelog](HoneyDrunk.Web.Rest/HoneyDrunk.Web.Rest.Abstractions/CHANGELOG.md)
- [HoneyDrunk.Web.Rest.AspNetCore changelog](HoneyDrunk.Web.Rest/HoneyDrunk.Web.Rest.AspNetCore/CHANGELOG.md)

## [Unreleased]

### Added

- Root `CHANGELOG.md` for repository-level change tracking (2026-06-19 docs-sync remediation).

### Internal

- Onboarded Web.Rest to SonarQube Cloud (ADR-0011 D11) via a `sonarcloud` job in `pr.yml`.
- Enabled ADR-0044 Grid Review Runner request generation for repository PRs.
- Adopted HoneyDrunk.Standards / HoneyDrunk.Standards.Tests 0.2.9 for ADR-0047 testing alignment.
- Seeded the Web.Rest coverage baseline and wired the coverage baseline ratchet artifact.

## [0.5.0] - 2026-05-18 - Kernel-required request context

### Changed

- `AddRest()` now requires Kernel request context services from `AddHoneyDrunkNode()` before Web.Rest is registered.
- `CorrelationMiddleware` now requires a live Kernel `IOperationContext` and uses its `CorrelationId`; it no longer falls back to request headers or generated IDs without Kernel context.
- Consolidated duplicated `ApiResult` / `ApiResult<T>` failure factory construction while preserving public factory APIs and response shape.
- Updated Kernel.Abstractions to 0.7.0, Auth.AspNetCore to 0.4.0, Transport to 0.6.0, and Vault integration packages to 0.5.0.

## [0.4.0] - 2026-05-04 - Typed TenantId context

### Changed

- Updated `HoneyDrunk.Kernel.Abstractions` to 0.5.0 for typed `TenantId` context adoption and `HoneyDrunk.Transport` to 0.5.0.
- `RequestLoggingScopeMiddleware` now omits the `TenantId` log-scope property for Internal-tenant requests and emits the sanitized ULID string for non-Internal requests.

## [0.3.0] - 2026-04-25 - Vault and App Configuration bootstrap

### Added

- ADR-0005/0006 Web.Rest bootstrap support for env-var-driven Azure Key Vault and Azure App Configuration through `AddWebRestBootstrap()`.
- `/internal/vault/invalidate` endpoint mapping via `MapHoneyDrunkWebRestVaultInvalidationWebhook()` for Event Grid cache invalidation.
- Canary coverage for Key Vault `SecretNewVersionCreated` invalidation using `{Provider}--{Key}` secret names.

### Changed

- Updated `HoneyDrunk.Web.Rest.AspNetCore` to consume the Vault Azure Key Vault, App Configuration, and Event Grid provider packages.

## [0.2.0] - 2026-02-14 - Exception mapping and auth shaping

### Added

- Exception mapping for Kernel typed exceptions (`ValidationException` -> 400, `NotFoundException` -> 404, `ConcurrencyException` -> 409, `SecurityException` -> 403, `DependencyFailureException` -> 503).
- `JsonException` and `BadHttpRequestException` mapping to 400 Bad Request.
- `ExceptionMappingMiddleware` guard against `Response.HasStarted`.
- `CorrelationMiddleware` warning when Kernel and header correlation IDs differ.
- `RestAuthorizationResultHandler` fallback to `HttpContext.User.Identity.IsAuthenticated` and a `PolicyNotFound` authorization deny code.

### Changed

- Updated Kernel.Abstractions to 0.4.0, Transport to 0.4.0, and Auth.AspNetCore to 0.2.0.

## [0.1.0] - 2026-01-10 - Initial release

### Added

- `ApiResult` / `ApiResult<T>` result envelopes with standard status codes.
- `ApiError`, `ApiErrorCode`, `ApiErrorResponse`, `ValidationError` error types.
- `PageRequest` / `PageResult<T>` pagination primitives.
- Standard header names, media type constants, and telemetry tag constants.
- ASP.NET Core integration: `CorrelationMiddleware`, `ExceptionMappingMiddleware`, `RequestLoggingMiddleware`.
- `RestOptions` for configurable middleware behavior and fluent service registration with Kernel and Auth integration.

[Unreleased]: https://github.com/HoneyDrunkStudios/HoneyDrunk.Web.Rest/compare/v0.5.0...HEAD
[0.5.0]: https://github.com/HoneyDrunkStudios/HoneyDrunk.Web.Rest/releases/tag/v0.5.0
[0.4.0]: https://github.com/HoneyDrunkStudios/HoneyDrunk.Web.Rest/releases/tag/v0.4.0
[0.3.0]: https://github.com/HoneyDrunkStudios/HoneyDrunk.Web.Rest/releases/tag/v0.3.0
[0.2.0]: https://github.com/HoneyDrunkStudios/HoneyDrunk.Web.Rest/releases/tag/v0.2.0
[0.1.0]: https://github.com/HoneyDrunkStudios/HoneyDrunk.Web.Rest/releases/tag/v0.1.0
