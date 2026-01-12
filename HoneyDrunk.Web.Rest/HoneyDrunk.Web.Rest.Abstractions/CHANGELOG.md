# Changelog

All notable changes to HoneyDrunk.Web.Rest.Abstractions will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.0] - 2026-01-10

### Added

#### Results
- `ApiResult` - Non-generic result envelope with status, correlation, and error support
- `ApiResult<T>` - Generic result envelope with typed data payload
- `ApiResultStatus` - Enumeration of result statuses (Success, Failed, Unauthorized, Forbidden, NotFound, Conflict, ValidationFailed, Error)
- Factory methods for common scenarios: `Success()`, `Fail()`, `NotFound()`, `Unauthorized()`, `Forbidden()`, `Conflict()`, `InternalError()`

#### Errors
- `ApiError` - Single error representation with code, message, details, and target
- `ApiErrorCode` - Standard machine-readable error codes (GENERAL_ERROR, VALIDATION_FAILED, NOT_FOUND, UNAUTHORIZED, FORBIDDEN, CONFLICT, BAD_REQUEST, INTERNAL_ERROR, NOT_IMPLEMENTED, SERVICE_UNAVAILABLE)
- `ApiErrorResponse` - Complete error response envelope with correlation, trace ID, timestamp, and validation errors
- `ValidationError` - Field-level validation error with field name, message, and optional code
- Factory methods: `Create()`, `CreateValidationError()`, `CreateNotFound()`, `CreateUnauthorized()`, `CreateForbidden()`, `CreateInternalError()`

#### Paging
- `PageRequest` - Pagination request with page number, page size, and computed skip count
- `PageResult<T>` - Paginated result set with items, total count, and navigation properties (TotalPages, HasPreviousPage, HasNextPage)
- Default values: PageNumber=1, PageSize=20, MaxPageSize=100

#### Constants
- `HeaderNames` - Standard HTTP header names (X-Correlation-Id, X-Request-Id, X-Trace-Id, X-Api-Version, X-RateLimit-Remaining, X-RateLimit-Reset)
- `MediaTypes` - Standard media types (application/json, application/problem+json, text/plain, application/octet-stream)

#### Telemetry
- `RestTelemetryTags` - Standard telemetry tag names for observability (correlation.id, request.id, http.method, http.route, http.status_code, error.type, error.code, user.id, tenant.id, service.name, service.version, request.duration_ms)

### Notes
- All contracts are pure .NET with no external dependencies
- Designed for use in any .NET project without requiring ASP.NET Core
- All types are immutable records for thread safety
- JSON serialization attributes included for consistent output

[0.1.0]: https://github.com/HoneyDrunkStudios/HoneyDrunk.Web.Rest/releases/tag/v0.1.0
