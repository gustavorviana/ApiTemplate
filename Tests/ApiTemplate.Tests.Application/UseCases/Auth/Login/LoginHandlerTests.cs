using Viana.Results;
using ApiTemplate.Application.Core.Entities;
using ApiTemplate.Application.Interfaces;
using ApiTemplate.Application.UseCases.Auth.Login;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace ApiTemplate.Tests.Application.UseCases.Auth.Login;

public class LoginHandlerTests
{
    private static IDbContext NewDb(IEnumerable<User>? users = null, IEnumerable<RefreshToken>? tokens = null)
    {
        var usersSet = (users ?? new List<User>()).ToList().BuildMockDbSet();
        var tokensSet = (tokens ?? new List<RefreshToken>()).ToList().BuildMockDbSet();
        var db = Substitute.For<IDbContext>();
        db.Users.Returns(usersSet);
        db.RefreshTokens.Returns(tokensSet);
        return db;
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnUnauthorized_WhenUserNotFound()
    {
        var db = NewDb();
        var handler = new LoginHandler(db, Substitute.For<IJwtService>(), Substitute.For<IPasswordHasher>());

        var result = await handler.ExecuteAsync(
            new LoginRequest { Email = "user@example.com", Password = "password" },
            CancellationToken.None);

        Assert.NotNull(Assert.IsType<Result<LoginResponse>>(result).Problem);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnUnauthorized_WhenPasswordDoesNotMatch()
    {
        var user = User.Create("User", "user@example.com", "stored-hash");
        var db = NewDb(users: new[] { user });

        var passwordHasher = Substitute.For<IPasswordHasher>();
        passwordHasher.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        var handler = new LoginHandler(db, Substitute.For<IJwtService>(), passwordHasher);

        var result = await handler.ExecuteAsync(
            new LoginRequest { Email = user.Email, Password = "wrong" },
            CancellationToken.None);

        Assert.NotNull(Assert.IsType<Result<LoginResponse>>(result).Problem);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCreateTokens_WhenCredentialsAreValid()
    {
        var user = User.Create("User", "user@example.com", "hashed-password");
        var db = NewDb(users: new[] { user });

        var jwtService = Substitute.For<IJwtService>();
        jwtService.GenerateAccessToken(user).Returns("access-token");

        var refreshToken = RefreshToken.Create(user.Id, "refresh-token", DateTime.UtcNow.AddDays(7));
        jwtService.GenerateRefreshToken(user.Id).Returns(refreshToken);

        var passwordHasher = Substitute.For<IPasswordHasher>();
        passwordHasher.Verify("password", "hashed-password").Returns(true);

        var handler = new LoginHandler(db, jwtService, passwordHasher);

        var result = await handler.ExecuteAsync(
            new LoginRequest { Email = user.Email, Password = "password" },
            CancellationToken.None);

        var response = Assert.IsType<Result<LoginResponse>>(result).Data;
        Assert.Equal("access-token", response.AccessToken);
        Assert.Equal("refresh-token", response.RefreshToken);
        await db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
