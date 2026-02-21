#if (EnableResult)
using ApiTemplate.Application.Results;
#endif
using ApiTemplate.Application.Interfaces;

namespace ApiTemplate.Application.UseCases.Auth.Login;

#if (EnableResult)
public class LoginHandle : IUseCaseHandle<LoginRequest, Result<LoginResponse>>
#else
public class LoginHandle : IUseCaseHandle<LoginRequest, LoginResponse?>
#endif
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public LoginHandle(
        IUserRepository userRepository,
        IJwtService jwtService,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _refreshTokenRepository = refreshTokenRepository;
    }

#if (EnableResult)
    public async Task<Result<LoginResponse>> ExecuteAsync(
#else
    public async Task<LoginResponse?> ExecuteAsync(
#endif
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
#if (EnableResult)
            return new ProblemResult(401, "Invalid email or password.");
#else
            return null;
#endif
        }

        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken(user.Id);

        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);

        var response = new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = refreshToken.ExpiresAt
        };

#if (EnableResult)
        return new Result<LoginResponse>(200, response);
#else
        return response;
#endif
    }
}
