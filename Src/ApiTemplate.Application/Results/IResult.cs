namespace ApiTemplate.Application.Results;

public interface IResult
{
	/// <summary>
	/// HTTP status code of the result.
	/// </summary>
	int Status { get; }

	/// <summary>
	/// Problem details when the result represents an error; otherwise null.
	/// </summary>
	ProblemResult? Problem { get; }
}

public interface IResult<out T> : IResult
{
    T? Data { get; }
}