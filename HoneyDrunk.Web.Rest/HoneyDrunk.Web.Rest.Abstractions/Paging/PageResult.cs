namespace HoneyDrunk.Web.Rest.Abstractions.Paging;

/// <summary>
/// Represents a paginated result set.
/// </summary>
/// <typeparam name="T">The type of items in the result.</typeparam>
public sealed record PageResult<T>
{
    /// <summary>
    /// Gets the items in the current page.
    /// </summary>
    public required IReadOnlyList<T> Items { get; init; }

    /// <summary>
    /// Gets the current page number (1-based).
    /// </summary>
    public required int PageNumber { get; init; }

    /// <summary>
    /// Gets the page size.
    /// </summary>
    public required int PageSize { get; init; }

    /// <summary>
    /// Gets the total number of items across all pages.
    /// </summary>
    public required long TotalCount { get; init; }

    /// <summary>
    /// Gets the total number of pages.
    /// </summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;

    /// <summary>
    /// Gets a value indicating whether there is a previous page.
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Gets a value indicating whether there is a next page.
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// Creates an empty <see cref="PageResult{T}"/>.
    /// </summary>
    /// <param name="pageNumber">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <returns>An empty page result.</returns>
#pragma warning disable CA1000 // Do not declare static members on generic types - factory methods are idiomatic for records
    public static PageResult<T> Empty(int pageNumber = 1, int pageSize = PageRequest.DefaultPageSize) => new()
    {
        Items = [],
        PageNumber = pageNumber,
        PageSize = pageSize,
        TotalCount = 0,
    };

    /// <summary>
    /// Creates a <see cref="PageResult{T}"/> from the specified items and pagination info.
    /// </summary>
    /// <param name="items">The items for this page.</param>
    /// <param name="pageNumber">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="totalCount">The total count of items.</param>
    /// <returns>A new page result.</returns>
    public static PageResult<T> Create(
        IReadOnlyList<T> items,
        int pageNumber,
        int pageSize,
        long totalCount) => new()
    {
        Items = items,
        PageNumber = pageNumber,
        PageSize = pageSize,
        TotalCount = totalCount,
    };
#pragma warning restore CA1000
}
