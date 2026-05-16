using ApiTemplate.Application.Core.ValueObjects;
using ApiTemplate.Application.Interfaces;
using ApiTemplate.Application.MessagesCatalog;
using Microsoft.EntityFrameworkCore;
using Viana.Results;

namespace ApiTemplate.Application.UseCases.Auth.RefreshToken;

public class RefreshTokenHandler(
    IAppDbContextFactory dbFactory,
    IJwtService jwtService) : IUseCaseHandler<RefreshTokenRequest, Result<RefreshTokenResponse>>
{
    public async Task<Result<RefreshTokenResponse>> ExecuteAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        await using var db = dbFactory.Create();

        var storedToken = await db.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == request.RefreshToken, cancellationToken);

        if (storedToken is null
            || storedToken.IsRevoked
            || storedToken.ExpiresAt <= DateTime.UtcNow)
        {
            return new ProblemResult(401, Messages.Auth.InvalidOrExpiredRefreshToken);
        }

        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Id == storedToken.UserId, cancellationToken);

        if (user is null)
            return new ProblemResult(401, Messages.Auth.UserNotFound);

        storedToken.IsRevoked = true;

        var newAccessToken = jwtService.GenerateAccessToken(new JwtClaimsContext(user.Id, user.Name, user.Email));
        var newRefreshToken = jwtService.GenerateRefreshToken(user.Id);

        db.RefreshTokens.Add(newRefreshToken);
        await db.SaveChangesAsync(cancellationToken);

        return new Result<RefreshTokenResponse>(new RefreshTokenResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken.Token,
            ExpiresAt = newRefreshToken.ExpiresAt
        });
    }
}
