#if (EnableResult)
using ApiTemplate.Application.Results;

namespace ApiTemplate.Application.UseCases;

public interface IUseCaseHandle<in TRequest, TResponse> where TResponse : IResult
#else
namespace ApiTemplate.Application.UseCases;

public interface IUseCaseHandle<in TRequest, TResponse>
#endif
{
    Task<TResponse> ExecuteAsync(TRequest request, CancellationToken cancellationToken = default);
}
