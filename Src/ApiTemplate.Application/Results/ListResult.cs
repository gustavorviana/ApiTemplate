namespace ApiTemplate.Application.Results;

/// <summary>
/// Result type for operations that return a list of items.
/// </summary>
/// <typeparam name="T">Type of each item in the list.</typeparam>
public class ListResult<T>(IReadOnlyList<T> data, ProblemResult? problem = null) : Result(problem?.Status ?? 200, problem), IListResult<T>
{
    public IReadOnlyList<T> Data => data;

    public static implicit operator ListResult<T>(ProblemResult problem) => new([], problem);

	public static implicit operator ListResult<T>(List<T> items) => new(items);
}