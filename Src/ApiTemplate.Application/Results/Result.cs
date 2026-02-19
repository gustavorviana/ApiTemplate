namespace ApiTemplate.Application.Results;

public record Result(int Status) : IResult;

public record Result<T>(int Status, T? Data, ProblemResult? Problem = null) : Result(Status)
{
    public static implicit operator Result<T>(ProblemResult problem)
    {
        return new Result<T>(problem.Status, default, problem);
    }
}