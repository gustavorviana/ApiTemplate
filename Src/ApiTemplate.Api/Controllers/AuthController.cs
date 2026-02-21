using ApiTemplate.Application.UseCases.Auth.Login;
using ApiTemplate.Application.UseCases.Auth.RefreshToken;
using Microsoft.AspNetCore.Mvc;
#if (EnableResult)
using IResult = ApiTemplate.Application.Results.IResult;
#endif
#if (UseDatabase)
using ApiTemplate.Application.UseCases.Auth.Register;
#endif

namespace ApiTemplate.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
#if (UseDatabase)
    [HttpPost("register")]
#if (EnableResult)
    public async Task<IResult> Register(
#else
    public async Task<IActionResult> Register(
#endif
        [FromServices] RegisterHandle handle,
        [FromBody] RegisterRequest request,
        CancellationToken token)
    {
#if (EnableResult)
        return await handle.ExecuteAsync(request, token);
#else
        var result = await handle.ExecuteAsync(request, token);
        return result is null ? Conflict() : StatusCode(201, result);
#endif
    }

#endif
    [HttpPost("login")]
#if (EnableResult)
    public async Task<IResult> Login(
#else
    public async Task<IActionResult> Login(
#endif
        [FromServices] LoginHandle handle,
        [FromBody] LoginRequest request,
        CancellationToken token)
    {
#if (EnableResult)
        return await handle.ExecuteAsync(request, token);
#else
        var result = await handle.ExecuteAsync(request, token);
        return result is null ? Unauthorized() : Ok(result);
#endif
    }

    [HttpPost("refresh")]
#if (EnableResult)
    public async Task<IResult> Refresh(
#else
    public async Task<IActionResult> Refresh(
#endif
        [FromServices] RefreshTokenHandle handle,
        [FromBody] RefreshTokenRequest request,
        CancellationToken token)
    {
#if (EnableResult)
        return await handle.ExecuteAsync(request, token);
#else
        var result = await handle.ExecuteAsync(request, token);
        return result is null ? Unauthorized() : Ok(result);
#endif
    }
}
