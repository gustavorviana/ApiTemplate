using ApiTemplate.Application.Core.Entities;
using ApiTemplate.Application.Core.Exceptions;
using ApiTemplate.Application.UseCases.Auth.Refresh;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ApiTemplate.Application.Core.Helper
{
    public class JwtHelper(IConfiguration configuration)
    {
        //User Test
        public static User user = new() { Id = 1, Email = "teste", PasswordHash = "AQAAAAIAAYagAAAAEJZxxYkvl/YENzZTXLsXzVQqMwZuDSkLHyh1OMg7jtc/zYaLGdYuR5tTOy9BtQhOpA==", Role = "User" };

        public string GenerateToken(User user)
        {
            var claims = new Dictionary<string, object>
            {
                [ClaimTypes.NameIdentifier] = user.Id,
                [ClaimTypes.Email] = user.Email,
                [ClaimTypes.Role] = user.Role
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
                Expires = DateTime.UtcNow.AddMinutes(5),
                SigningCredentials = creds
            };

            return new JsonWebTokenHandler().CreateToken(tokenDescriptor);
        }

        public string GenerateAndSaveRefreshToken(User user)
        {
            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            //save user to database here
            return refreshToken;
        }

        public User ValidateRefreshToken(RefreshTokenRequest request)
        {
            if (user.Id != request.UserId ||user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                throw new BadRequestException("Invalid user or refresh token");

            return user;
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
