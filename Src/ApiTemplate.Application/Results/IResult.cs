namespace ApiTemplate.Application.Results;

public interface IResult
{
    int Status { get; }
}

public interface IResult<out T> : IResult
{
    T? Value { get; }
}