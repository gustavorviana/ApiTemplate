using Viana.Results;
using System.Security.Claims;
using ApiTemplate.Application.Interfaces;
using ApiTemplate.Application.MessagesCatalog;

namespace ApiTemplate.Application.UseCases.Auth.RefreshToken;

public class RefreshTokenHandle : IUseCaseHandle<RefreshTokenRequest, Result<RefreshTokenResponse>>
{
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;

    public RefreshTokenHandle(
        IJwtService jwtService,
        IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository)
    {
        _jwtService = jwtService;
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
    }

    public async Task<Result<RefreshTokenResponse>> ExecuteAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        var principal = _jwtService.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal is null)
        {
            return new ProblemResult(401, Messages.Auth.InvalidAccessToken);
        }

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)
                          ?? principal.FindFirst("sub");

        if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return new ProblemResult(401, Messages.Auth.InvalidAccessToken);
        }

        var storedToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);

        if (storedToken is null || !storedToken.IsActive || storedToken.UserId != userId)
        {
            return new ProblemResult(401, Messages.Auth.InvalidOrExpiredRefreshToken);
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return new ProblemResult(401, Messages.Auth.UserNotFound);
        }

        await _refreshTokenRepository.RevokeAsync(storedToken, cancellationToken);

        var newAccessToken = _jwtService.GenerateAccessToken(user);
        var newRefreshToken = _jwtService.GenerateRefreshToken(userId);

        await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);

        var response = new RefreshTokenResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken.Token,
            ExpiresAt = newRefreshToken.ExpiresAt
        };

        return new Result<RefreshTokenResponse>(response);
    }
}
