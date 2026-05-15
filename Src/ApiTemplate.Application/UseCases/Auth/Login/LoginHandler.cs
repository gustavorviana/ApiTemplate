using ApiTemplate.Application.Interfaces;
using ApiTemplate.Application.MessagesCatalog;
using Microsoft.EntityFrameworkCore;
using Viana.Results;

namespace ApiTemplate.Application.UseCases.Auth.Login;

public class LoginHandler(
    IDbContext db,
    IJwtService jwtService,
    IPasswordHasher passwordHasher) : IUseCaseHandler<LoginRequest, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> ExecuteAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
            return new ProblemResult(401, Messages.Auth.InvalidEmailOrPassword);

        var accessToken = jwtService.GenerateAccessToken(user);
        var refreshToken = jwtService.GenerateRefreshToken(user.Id);

        db.RefreshTokens.Add(refreshToken);
        await db.SaveChangesAsync(cancellationToken);

        var response = new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = refreshToken.ExpiresAt
        };

        return new Result<LoginResponse>(response);
    }
}
