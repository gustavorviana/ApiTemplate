namespace ApiTemplate.Application.Results;

/// <summary>
/// Result type for operations that return a paginated list of items.
/// </summary>
/// <typeparam name="T">Type of each item in the page.</typeparam>
public class PagedResult<T>(IReadOnlyList<T> data, int pageNumber, int totalPages, ProblemResult? problem = null) : ListResult<T>(data, problem), IPagedResult<T>
{
    public int PageNumber => pageNumber;
    public int TotalPages => totalPages;

    public static PagedResult<T> Create(IReadOnlyList<T> data, int pageNumber, int pageSize, int totalCount)
    {
        var totalPages = pageSize > 0 ? (int)Math.Ceiling(totalCount / (double)pageSize) : 0;
        return new PagedResult<T>(data, pageNumber, totalPages);
    }

    public static implicit operator PagedResult<T>(ProblemResult problem) => new([], 0, 0, problem);
}
