using System.Text.Json.Serialization;

namespace ApiTemplate.Application.Results;

public class Result(int status, ProblemResult? problem = null) : IResult
{
    public int Status => status;
    [JsonIgnore]
    public ProblemResult? Problem => problem;
}

public class Result<T>(int status, T? data, ProblemResult? problem = null) : Result(status, problem), IResult<T>
{
    public T? Data => data;

    public static implicit operator Result<T>(T data) => new(200, data);

    public static implicit operator Result<T>(ProblemResult problem) => new(problem.Status, default, problem);
}