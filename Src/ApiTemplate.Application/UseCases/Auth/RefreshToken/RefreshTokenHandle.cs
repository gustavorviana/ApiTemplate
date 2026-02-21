#if (EnableResult)
using ApiTemplate.Application.Results;
#endif
using System.Security.Claims;
using ApiTemplate.Application.Interfaces;

namespace ApiTemplate.Application.UseCases.Auth.RefreshToken;

#if (EnableResult)
public class RefreshTokenHandle : IUseCaseHandle<RefreshTokenRequest, Result<RefreshTokenResponse>>
#else
public class RefreshTokenHandle : IUseCaseHandle<RefreshTokenRequest, RefreshTokenResponse?>
#endif
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

#if (EnableResult)
    public async Task<Result<RefreshTokenResponse>> ExecuteAsync(
#else
    public async Task<RefreshTokenResponse?> ExecuteAsync(
#endif
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        var principal = _jwtService.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal is null)
        {
#if (EnableResult)
            return new ProblemResult(401, "Invalid access token.");
#else
            return null;
#endif
        }

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)
                          ?? principal.FindFirst("sub");

        if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
#if (EnableResult)
            return new ProblemResult(401, "Invalid access token.");
#else
            return null;
#endif
        }

        var storedToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);

        if (storedToken is null || !storedToken.IsActive || storedToken.UserId != userId)
        {
#if (EnableResult)
            return new ProblemResult(401, "Invalid or expired refresh token.");
#else
            return null;
#endif
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
#if (EnableResult)
            return new ProblemResult(401, "User not found.");
#else
            return null;
#endif
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

#if (EnableResult)
        return new Result<RefreshTokenResponse>(200, response);
#else
        return response;
#endif
    }
}
