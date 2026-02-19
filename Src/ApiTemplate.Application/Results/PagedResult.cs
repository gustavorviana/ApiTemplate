namespace ApiTemplate.Application.Results;

/// <summary>
/// Result type for operations that return a paginated list of items.
/// </summary>
/// <typeparam name="T">Type of each item in the page.</typeparam>
public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int PageNumber,
    int TotalPages,
    ProblemResult? Problem = null) : ListResult<T>(Items, Problem), IPagedResult<T>
{
    public static PagedResult<T> Create(IReadOnlyList<T> items, int pageNumber, int pageSize, int totalCount)
    {
        var totalPages = pageSize > 0 ? (int)Math.Ceiling(totalCount / (double)pageSize) : 0;
        return new PagedResult<T>(items, pageNumber, totalPages);
    }

    public static implicit operator PagedResult<T>(ProblemResult problem) => new([], 0, 0, problem);
}