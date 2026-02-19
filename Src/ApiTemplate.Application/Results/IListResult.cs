namespace ApiTemplate.Application.Results;

/// <summary>
/// Result that contains a list of items.
/// </summary>
/// <typeparam name="T">Type of each item in the list.</typeparam>
public interface IListResult<out T> : IResult<IReadOnlyList<T>>;