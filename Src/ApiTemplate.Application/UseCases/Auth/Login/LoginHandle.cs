using ApiTemplate.Application.Core.Entities;
using ApiTemplate.Application.Core.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace ApiTemplate.Application.UseCases.Auth.Login
{
    public class LoginHandle(IConfiguration configuration) : IUseCaseHandle<AuthRequest, AuthResponse>
    {
        //User Test
        public static User user = new() { Email = "teste", PasswordHash = "AQAAAAIAAYagAAAAEJZxxYkvl/YENzZTXLsXzVQqMwZuDSkLHyh1OMg7jtc/zYaLGdYuR5tTOy9BtQhOpA==" };

        public Task<AuthResponse> ExecuteAsync(AuthRequest request, CancellationToken cancellationToken = default)
        {
            if (user.Email != request.Email || new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
                throw new BadRequestException("User or password is incorrect");

            return Task.FromResult(new AuthResponse { Token = GenerateToken(user) });
        }

        private string GenerateToken(User user)
        {
            var claims = new Dictionary<string, object>
            {
                [ClaimTypes.Name] = user.Email
            };

            var issuer = configuration.GetSection("JwtSettings:Issuer").Value;
            var audience = configuration.GetSection("JwtSettings:Audience").Value;
            var secretKey = configuration.GetSection("JwtSettings:SecretKey").Value;

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = issuer,
                Audience = audience,
                Claims = claims,
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = creds
            };

            return new JsonWebTokenHandler().CreateToken(tokenDescriptor);
        }
    }
}
