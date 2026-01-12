namespace HoneyDrunk.Web.Rest.Abstractions.Paging;

/// <summary>
/// Represents a pagination request using page number and page size.
/// This uses offset-based pagination for simplicity and broad compatibility.
/// For large datasets or real-time data, consider cursor-based pagination.
/// </summary>
public sealed record PageRequest
{
    /// <summary>
    /// The default page number (1-based).
    /// </summary>
    public const int DefaultPageNumber = 1;

    /// <summary>
    /// The default page size.
    /// </summary>
    public const int DefaultPageSize = 20;

    /// <summary>
    /// The maximum allowed page size.
    /// </summary>
    public const int MaxPageSize = 100;

    /// <summary>
    /// Gets the page number (1-based).
    /// </summary>
    public int PageNumber { get; init; } = DefaultPageNumber;

    /// <summary>
    /// Gets the number of items per page.
    /// </summary>
    public int PageSize { get; init; } = DefaultPageSize;

    /// <summary>
    /// Gets the calculated skip count for database queries.
    /// </summary>
    public int Skip => (NormalizedPageNumber - 1) * NormalizedPageSize;

    /// <summary>
    /// Gets the normalized page number (ensures minimum of 1).
    /// </summary>
    public int NormalizedPageNumber => Math.Max(1, PageNumber);

    /// <summary>
    /// Gets the normalized page size (ensures between 1 and MaxPageSize).
    /// </summary>
    public int NormalizedPageSize => Math.Clamp(PageSize, 1, MaxPageSize);

    /// <summary>
    /// Creates a new <see cref="PageRequest"/> with default values.
    /// </summary>
    /// <returns>A new <see cref="PageRequest"/> instance.</returns>
    public static PageRequest Default() => new();

    /// <summary>
    /// Creates a new <see cref="PageRequest"/> with the specified values.
    /// </summary>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The page size.</param>
    /// <returns>A new <see cref="PageRequest"/> instance.</returns>
    public static PageRequest Create(int pageNumber, int pageSize) => new()
    {
        PageNumber = pageNumber,
        PageSize = pageSize,
    };
}
