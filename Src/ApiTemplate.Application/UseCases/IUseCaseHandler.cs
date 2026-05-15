using Viana.Results;

namespace ApiTemplate.Application.UseCases;

public interface IUseCaseHandler<in TRequest, TResponse> where TResponse : IResult
{
    Task<TResponse> ExecuteAsync(TRequest request, CancellationToken cancellationToken = default);
}
