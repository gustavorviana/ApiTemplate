using System.Security.Claims;
using ApiTemplate.Application.Core.Entities;

namespace ApiTemplate.Application.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    RefreshToken GenerateRefreshToken(Guid userId);
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
