namespace ApiTemplate.Application.Results;

/// <summary>
/// Result type for operations that return a list of items.
/// </summary>
/// <typeparam name="T">Type of each item in the list.</typeparam>
public record ListResult<T>(IReadOnlyList<T> Items, ProblemResult? Problem = null)
    : Result(Problem?.Status ?? 200, Problem), IListResult<T>
{
    IReadOnlyList<T>? IResult<IReadOnlyList<T>>.Data => Items;

    public static implicit operator ListResult<T>(ProblemResult problem) => new([], problem);

    public static implicit operator ListResult<T>(List<T> items) => new(items);
}