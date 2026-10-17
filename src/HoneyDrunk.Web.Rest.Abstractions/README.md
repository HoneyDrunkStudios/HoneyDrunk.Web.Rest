# HoneyDrunk.Web.Rest.Abstractions

Pure contracts and constants for HoneyDrunk REST services. This library has no dependencies on ASP.NET Core or any runtime packages.

## Contract Shapes

### ApiResult / ApiResult\<T\>

Standard response envelope for successful operations:

```json
{
  "status": "Success",
  "correlationId": "abc-123",
  "traceId": "00-abc123...",
  "timestamp": "2026-01-10T00:00:00Z",
  "data": { ... }
}
```

### ApiErrorResponse

Standard error envelope for all non-2xx responses:

```json
{
  "correlationId": "abc-123",
  "traceId": "00-abc123...",
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

### PageResult\<T\>

Paginated result set:

```json
{
  "items": [...],
  "pageNumber": 1,
  "pageSize": 20,
  "totalCount": 100,
  "totalPages": 5,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

## Constants

- `HeaderNames` - Standard HTTP header names (X-Correlation-Id, X-Request-Id, etc.)
- `MediaTypes` - Standard media types (application/json, etc.)
- `ApiErrorCode` - Machine-readable error codes
- `RestTelemetryTags` - Telemetry tag names for logging and tracing

## Usage

Reference this package for contract definitions. Use `HoneyDrunk.Web.Rest.AspNetCore` for the actual middleware and filters.
