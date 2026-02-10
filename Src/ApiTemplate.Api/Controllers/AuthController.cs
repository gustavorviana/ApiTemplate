using ApiTemplate.Application.UseCases.Auth;
using ApiTemplate.Application.UseCases.Auth.Login;
using ApiTemplate.Application.UseCases.Auth.Refresh;
using Microsoft.AspNetCore.Mvc;

namespace ApiTemplate.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IConfiguration configuration) : ControllerBase
    {
        [HttpPost("login")]
        public Task<AuthResponse> Login([FromBody] AuthRequest request, CancellationToken cancellationToken)
            => new LoginHandle(configuration).ExecuteAsync(request, cancellationToken);

        [HttpPost("refresh-token")]
        public Task<AuthResponse> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
            => new RefreshTokenHandle(configuration).ExecuteAsync(request, cancellationToken);
    }
}