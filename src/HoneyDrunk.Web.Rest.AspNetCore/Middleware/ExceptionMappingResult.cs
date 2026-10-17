using System.Net;

namespace HoneyDrunk.Web.Rest.AspNetCore.Middleware;

/// <summary>
/// Represents the result of mapping an exception.
/// </summary>
internal readonly record struct ExceptionMappingResult(HttpStatusCode StatusCode, string ErrorCode, string Message);
