using ApiTemplate.Application.UseCases.Auth.Login;
using ApiTemplate.Application.UseCases.Auth.RefreshToken;
using Microsoft.AspNetCore.Mvc;
using IResult = Viana.Results.IResult;
using Viana.Results;
#if (UseDatabase)
using ApiTemplate.Application.UseCases.Auth.Register;
using ApiTemplate.Application.UseCases.Auth.Login;
#endif

namespace ApiTemplate.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
#if (UseDatabase)
    [HttpPost("register")]
    public async Task<IResult<RegisterResponse>> Register(
        [FromServices] RegisterHandle handle,
        [FromBody] RegisterRequest request,
        CancellationToken token)
    {
        return await handle.ExecuteAsync(request, token);
    }

#endif
    [HttpPost("login")]
    public async Task<IResult<LoginResponse>> Login(
        [FromServices] LoginHandle handle,
        [FromBody] LoginRequest request,
        CancellationToken token)
    {
        return await handle.ExecuteAsync(request, token);
    }

    [HttpPost("refresh")]
    public async Task<IResult> Refresh(
        [FromServices] RefreshTokenHandle handle,
        [FromBody] RefreshTokenRequest request,
        CancellationToken token)
    {
        return await handle.ExecuteAsync(request, token);
    }
}
