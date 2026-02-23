using ApiTemplate.Application.Core.Entities;
using System.Security.Claims;

namespace ApiTemplate.Application.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    RefreshToken GenerateRefreshToken(Guid userId);
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
