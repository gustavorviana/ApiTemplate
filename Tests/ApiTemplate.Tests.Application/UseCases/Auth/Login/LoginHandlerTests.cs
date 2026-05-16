using Viana.Results;
using ApiTemplate.Application.Core.Entities;
using ApiTemplate.Application.Core.ValueObjects;
using ApiTemplate.Application.Interfaces;
using ApiTemplate.Application.UseCases.Auth.Login;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace ApiTemplate.Tests.Application.UseCases.Auth.Login;

public class LoginHandlerTests
{
    private static IAppDbContextFactory NewFactory(IEnumerable<UserEntity>? users = null, IEnumerable<RefreshTokenEntity>? tokens = null)
    {
        var usersSet = (users ?? new List<UserEntity>()).ToList().BuildMockDbSet();
        var tokensSet = (tokens ?? new List<RefreshTokenEntity>()).ToList().BuildMockDbSet();
        var db = Substitute.For<IAppDbContext>();
        db.Users.Returns(usersSet);
        db.RefreshTokens.Returns(tokensSet);

        var factory = Substitute.For<IAppDbContextFactory>();
        factory.Create().Returns(db);
        return factory;
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
        var factory = NewFactory();
        var handler = new LoginHandler(factory, Substitute.For<IJwtService>(), Substitute.For<IPasswordHasher>());

        var result = await handler.ExecuteAsync(
            new LoginRequest { Email = "user@example.com", Password = "password" },
            CancellationToken.None);

        Assert.NotNull(Assert.IsType<Result<LoginResponse>>(result).Problem);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnUnauthorized_WhenPasswordDoesNotMatch()
    {
        var user = NewUser();
        var factory = NewFactory(users: new[] { user });

        var passwordHasher = Substitute.For<IPasswordHasher>();
        passwordHasher.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        var handler = new LoginHandler(factory, Substitute.For<IJwtService>(), passwordHasher);

        var result = await handler.ExecuteAsync(
            new LoginRequest { Email = user.Email, Password = "wrong" },
            CancellationToken.None);

        Assert.NotNull(Assert.IsType<Result<LoginResponse>>(result).Problem);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCreateTokens_WhenCredentialsAreValid()
    {
        var user = NewUser(hash: "hashed-password");
        var factory = NewFactory(users: new[] { user });

        var jwtService = Substitute.For<IJwtService>();
        jwtService.GenerateAccessToken(Arg.Any<JwtClaimsContext>()).Returns("access-token");

        var refreshToken = new RefreshTokenEntity
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = "refresh-token",
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        jwtService.GenerateRefreshToken(user.Id).Returns(refreshToken);

        var passwordHasher = Substitute.For<IPasswordHasher>();
        passwordHasher.Verify("password", "hashed-password").Returns(true);

        var handler = new LoginHandler(factory, jwtService, passwordHasher);

        var result = await handler.ExecuteAsync(
            new LoginRequest { Email = user.Email, Password = "password" },
            CancellationToken.None);

        var response = Assert.IsType<Result<LoginResponse>>(result).Data;
        Assert.Equal("access-token", response.AccessToken);
        Assert.Equal("refresh-token", response.RefreshToken);
    }
}
