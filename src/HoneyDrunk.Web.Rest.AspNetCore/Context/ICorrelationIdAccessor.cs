namespace HoneyDrunk.Web.Rest.AspNetCore.Context;

/// <summary>
/// Provides access to the current correlation ID for the request.
/// </summary>
public interface ICorrelationIdAccessor
{
    /// <summary>
    /// Gets the current correlation ID.
    /// </summary>
    string? CorrelationId { get; }

    /// <summary>
    /// Sets the correlation ID for the current request.
    /// </summary>
    /// <param name="correlationId">The correlation ID to set.</param>
    void SetCorrelationId(string correlationId);
}
