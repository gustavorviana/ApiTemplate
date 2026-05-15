using System.Security.Claims;
using ApiTemplate.Application.Interfaces;
using ApiTemplate.Application.MessagesCatalog;
using Microsoft.EntityFrameworkCore;
using Viana.Results;

namespace ApiTemplate.Application.UseCases.Auth.RefreshToken;

public class RefreshTokenHandler(
    IDbContext db,
    IJwtService jwtService) : IUseCaseHandler<RefreshTokenRequest, Result<RefreshTokenResponse>>
{
    public async Task<Result<RefreshTokenResponse>> ExecuteAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        var principal = jwtService.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal is null)
            return new ProblemResult(401, Messages.Auth.InvalidAccessToken);

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)
                          ?? principal.FindFirst("sub");

        if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId))
            return new ProblemResult(401, Messages.Auth.InvalidAccessToken);

        var storedToken = await db.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == request.RefreshToken, cancellationToken);

        if (storedToken is null || !storedToken.IsActive || storedToken.UserId != userId)
            return new ProblemResult(401, Messages.Auth.InvalidOrExpiredRefreshToken);

        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
            return new ProblemResult(401, Messages.Auth.UserNotFound);

        storedToken.Revoke();

        var newAccessToken = jwtService.GenerateAccessToken(user);
        var newRefreshToken = jwtService.GenerateRefreshToken(userId);

        db.RefreshTokens.Add(newRefreshToken);
        await db.SaveChangesAsync(cancellationToken);

        var response = new RefreshTokenResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken.Token,
            ExpiresAt = newRefreshToken.ExpiresAt
        };

        return new Result<RefreshTokenResponse>(response);
    }
}
