using Viana.Results;

namespace ApiTemplate.Application.UseCases;

public interface IUseCaseHandle<in TRequest, TResponse> where TResponse : IResult;