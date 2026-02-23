using Viana.Results;
using ApiTemplate.Application.Core.Entities;
using ApiTemplate.Application.Interfaces;
using ApiTemplate.Application.UseCases.Auth.Login;
using NSubstitute;

namespace ApiTemplate.Tests.Application.UseCases.Auth.Login;

public class LoginHandleTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldReturnUnauthorized_WhenUserNotFound()
    {
        var userRepository = Substitute.For<IUserRepository>();
        userRepository
            .GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var jwtService = Substitute.For<IJwtService>();
        var refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
        var passwordHasher = Substitute.For<IPasswordHasher>();

        var handle = new LoginHandle(
            userRepository,
            jwtService,
            refreshTokenRepository,
            passwordHasher);

        var request = new LoginRequest
        {
            Email = "user@example.com",
            Password = "password"
        };

        var result = await handle.ExecuteAsync(request, CancellationToken.None);

        var typedResult = Assert.IsType<Result<LoginResponse>>(result);
        Assert.NotNull(typedResult.Problem);

        await refreshTokenRepository
            .DidNotReceive()
            .AddAsync(Arg.Any<RefreshToken>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnUnauthorized_WhenPasswordDoesNotMatch()
    {
        var storedPasswordHash = "stored-hash";
        var user = User.Create("User", "user@example.com", storedPasswordHash);

        var userRepository = Substitute.For<IUserRepository>();
        userRepository
            .GetByEmailAsync(user.Email, Arg.Any<CancellationToken>())
            .Returns(user);

        var jwtService = Substitute.For<IJwtService>();
        var refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
        var passwordHasher = Substitute.For<IPasswordHasher>();
        passwordHasher
            .Verify(Arg.Any<string>(), Arg.Any<string>())
            .Returns(false);

        var handle = new LoginHandle(
            userRepository,
            jwtService,
            refreshTokenRepository,
            passwordHasher);

        var request = new LoginRequest
        {
            Email = user.Email,
            Password = "wrong-password"
        };

        var result = await handle.ExecuteAsync(request, CancellationToken.None);

        var typedResult = Assert.IsType<Result<LoginResponse>>(result);
        Assert.NotNull(typedResult.Problem);

        await refreshTokenRepository
            .DidNotReceive()
            .AddAsync(Arg.Any<RefreshToken>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCreateTokens_WhenCredentialsAreValid()
    {
        var password = "password";
        var passwordHash = "hashed-password";
        var user = User.Create("User", "user@example.com", passwordHash);

        var userRepository = Substitute.For<IUserRepository>();
        userRepository
            .GetByEmailAsync(user.Email, Arg.Any<CancellationToken>())
            .Returns(user);

        var jwtService = Substitute.For<IJwtService>();
        jwtService
            .GenerateAccessToken(user)
            .Returns("access-token");

        var refreshToken = RefreshToken.Create(
            user.Id,
            "refresh-token",
            DateTime.UtcNow.AddDays(7));

        jwtService
            .GenerateRefreshToken(user.Id)
            .Returns(refreshToken);

        var refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
        var passwordHasher = Substitute.For<IPasswordHasher>();
        passwordHasher
            .Verify(password, passwordHash)
            .Returns(true);

        var handle = new LoginHandle(
            userRepository,
            jwtService,
            refreshTokenRepository,
            passwordHasher);

        var request = new LoginRequest
        {
            Email = user.Email,
            Password = password
        };

        var result = await handle.ExecuteAsync(request, CancellationToken.None);

        var response = Assert.IsType<Result<LoginResponse>>(result).Data;

        Assert.Equal("access-token", response.AccessToken);
        Assert.Equal("refresh-token", response.RefreshToken);

        await refreshTokenRepository
            .Received(1)
            .AddAsync(Arg.Any<RefreshToken>(), Arg.Any<CancellationToken>());
    }
}
