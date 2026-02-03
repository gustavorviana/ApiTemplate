namespace ApiTemplate.Application.UseCases;

public interface IUseCaseHandle<in TRequest, TResponse>
{
    Task<TResponse> ExecuteAsync(TRequest request, CancellationToken cancellationToken = default);
}