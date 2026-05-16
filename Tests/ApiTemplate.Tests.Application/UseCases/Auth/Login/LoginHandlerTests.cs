using Viana.Results;
using ApiTemplate.Application.Core.Entities;
using ApiTemplate.Application.Core.ValueObjects;
using ApiTemplate.Application.Interfaces;
using ApiTemplate.Application.UseCases.Auth.Login;
using ApiTemplate.Tests.Application.Base;
using NSubstitute;

namespace ApiTemplate.Tests.Application.UseCases.Auth.Login;

public class LoginHandlerTests : TestBase
{
    private readonly IJwtService _jwtService = Substitute.For<IJwtService>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly LoginHandler _handler;

    public LoginHandlerTests()
    {
        _handler = new LoginHandler(AppDbContextFactory, _jwtService, _passwordHasher);
    }

    private static UserEntity NewUser(string email = "user@example.com", string hash = "stored-hash") =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = "User",
            Email = email,
            PasswordHash = hash
        };

    [Fact]
    public async Task ExecuteAsync_ShouldReturnUnauthorized_WhenUserNotFound()
    {
        var usersDbSet = ToMockDbSet(Array.Empty<UserEntity>());
        var tokensDbSet = ToMockDbSet(Array.Empty<RefreshTokenEntity>());
        AppContext.Users.Returns(usersDbSet);
        AppContext.RefreshTokens.Returns(tokensDbSet);

        var result = await _handler.ExecuteAsync(
            new LoginRequest { Email = "user@example.com", Password = "password" },
            CancellationToken.None);

        Assert.NotNull(Assert.IsType<Result<LoginResponse>>(result).Problem);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnUnauthorized_WhenPasswordDoesNotMatch()
    {
        var user = NewUser();
        var usersDbSet = ToMockDbSet(new[] { user });
        var tokensDbSet = ToMockDbSet(Array.Empty<RefreshTokenEntity>());
        AppContext.Users.Returns(usersDbSet);
        AppContext.RefreshTokens.Returns(tokensDbSet);
        _passwordHasher.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        var result = await _handler.ExecuteAsync(
            new LoginRequest { Email = user.Email, Password = "wrong" },
            CancellationToken.None);

        Assert.NotNull(Assert.IsType<Result<LoginResponse>>(result).Problem);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCreateTokens_WhenCredentialsAreValid()
    {
        var user = NewUser(hash: "hashed-password");
        var usersDbSet = ToMockDbSet(new[] { user });
        var tokensDbSet = ToMockDbSet(Array.Empty<RefreshTokenEntity>());
        AppContext.Users.Returns(usersDbSet);
        AppContext.RefreshTokens.Returns(tokensDbSet);

        _jwtService.GenerateAccessToken(Arg.Any<JwtClaimsContext>()).Returns("access-token");
        var refreshToken = new RefreshTokenEntity
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = "refresh-token",
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        _jwtService.GenerateRefreshToken(user.Id).Returns(refreshToken);
        _passwordHasher.Verify("password", "hashed-password").Returns(true);

        var result = await _handler.ExecuteAsync(
            new LoginRequest { Email = user.Email, Password = "password" },
            CancellationToken.None);

        var response = Assert.IsType<Result<LoginResponse>>(result).Data;
        Assert.Equal("access-token", response.AccessToken);
        Assert.Equal("refresh-token", response.RefreshToken);
    }
}
