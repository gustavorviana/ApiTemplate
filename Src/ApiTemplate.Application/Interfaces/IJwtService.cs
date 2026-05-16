using ApiTemplate.Application.Core.Entities;
using ApiTemplate.Application.Core.ValueObjects;

namespace ApiTemplate.Application.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(JwtClaimsContext claims);
    RefreshTokenEntity GenerateRefreshToken(Guid userId);
}
