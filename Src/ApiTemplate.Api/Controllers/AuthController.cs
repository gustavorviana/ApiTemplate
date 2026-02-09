using ApiTemplate.Application.Core.Entities;
using ApiTemplate.Application.UseCases.Auth;
using ApiTemplate.Application.UseCases.Auth.Login;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace ApiTemplate.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IConfiguration configuration) : ControllerBase
    {
        [HttpPost("login")]
        public Task<AuthResponse> Login([FromBody] AuthRequest request, CancellationToken cancellationToken)
            => new LoginHandle(configuration).ExecuteAsync(request, cancellationToken);

        [Authorize]
        [HttpGet()]
        public IActionResult AuthenticationOnlyEndpoint()
        {
            return Ok("You Are Authenticated");
        }
    }
}