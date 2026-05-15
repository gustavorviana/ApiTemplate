using ApiTemplate.Application.UseCases.Auth.Login;
using ApiTemplate.Application.UseCases.Auth.Register;
using ApiTemplate.Application.UseCases.Auth.RefreshToken;
using Microsoft.AspNetCore.Mvc;
#if EnableRateLimiting
using ApiTemplate.Api.Extensions;
using Microsoft.AspNetCore.RateLimiting;
#endif
using IResult = Viana.Results.IResult;
using Viana.Results;

namespace ApiTemplate.Api.Controllers;

[ApiController]
[Route("[controller]")]
#if (UseUseCase)
public class AuthController(
    LoginHandler loginHandler,
    RegisterHandler registerHandler,
    RefreshTokenHandler refreshTokenHandler) : ControllerBase
#else
public class AuthController : ControllerBase
#endif
{
    [HttpPost("register")]
#if EnableRateLimiting
    [DisableRateLimiting]
#endif
    public async Task<IResult<RegisterResponse>> Register(
#if (UseCqrs)
        [FromServices] RegisterHandler handler,
#endif
        [FromBody] RegisterRequest request,
        CancellationToken token)
#if (UseCqrs)
        => await handler.ExecuteAsync(request, token);
#elif (UseUseCase)
        => await registerHandler.ExecuteAsync(request, token);
#endif

    [HttpPost("login")]
#if EnableRateLimiting
    [EnableRateLimiting(RateLimitingExtensions.LoginPolicyName)]
#endif
    public async Task<IResult<LoginResponse>> Login(
#if (UseCqrs)
        [FromServices] LoginHandler handler,
#endif
        [FromBody] LoginRequest request,
        CancellationToken token)
#if (UseCqrs)
        => await handler.ExecuteAsync(request, token);
#elif (UseUseCase)
        => await loginHandler.ExecuteAsync(request, token);
#endif

    [HttpPost("refresh")]
    public async Task<IResult> Refresh(
#if (UseCqrs)
        [FromServices] RefreshTokenHandler handler,
#endif
        [FromBody] RefreshTokenRequest request,
        CancellationToken token)
#if (UseCqrs)
        => await handler.ExecuteAsync(request, token);
#elif (UseUseCase)
        => await refreshTokenHandler.ExecuteAsync(request, token);
#endif
}
