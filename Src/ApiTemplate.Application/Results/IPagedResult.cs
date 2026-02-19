namespace ApiTemplate.Application.Results;

/// <summary>
/// Result that contains a paginated list of items.
/// </summary>
/// <typeparam name="T">Type of each item in the page.</typeparam>
public interface IPagedResult<out T> : IListResult<T>
{
	/// <summary>
	/// Current page number (1-based).
	/// </summary>
	int PageNumber { get; }

	/// <summary>
	/// Total number of pages.
	/// </summary>
	int TotalPages { get; }
}
