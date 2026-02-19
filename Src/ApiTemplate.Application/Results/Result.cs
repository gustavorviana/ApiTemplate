namespace ApiTemplate.Application.Results;

public record Result(int Status, ProblemResult? Problem = null) : IResult;

public record Result<T>(int Status, T? Data, ProblemResult? Problem = null) : Result(Status, Problem), IResult<T>
{
    public static implicit operator Result<T>(T data)
    {
        return new Result<T>(200, data);
    }

    public static implicit operator Result<T>(ProblemResult problem)
    {
        return new Result<T>(problem.Status, default, problem);
    }
}