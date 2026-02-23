using ApiTemplate.Application.Interfaces;
using ApiTemplate.Application.MessagesCatalog;
using Viana.Results;

namespace ApiTemplate.Application.UseCases.Auth.Login;

public class LoginHandle : IUseCaseHandle<LoginRequest, Result<LoginResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;

    public LoginHandle(
        IUserRepository userRepository,
        IJwtService jwtService,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<LoginResponse>> ExecuteAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return new ProblemResult(401, Messages.Auth.InvalidEmailOrPassword);
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

        return new Result<LoginResponse>(response);
    }
}