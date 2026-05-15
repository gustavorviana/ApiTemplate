using System.Security.Claims;
using Viana.Results;
using ApiTemplate.Application.Core.Entities;
using ApiTemplate.Application.Interfaces;
using ApiTemplate.Application.UseCases.Auth.RefreshToken;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace ApiTemplate.Tests.Application.UseCases.Auth.RefreshTokens;

public class RefreshTokenHandlerTests
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
    public async Task ExecuteAsync_ShouldReturnUnauthorized_WhenAccessTokenIsInvalid()
    {
        var jwtService = Substitute.For<IJwtService>();
        jwtService.GetPrincipalFromExpiredToken(Arg.Any<string>()).Returns((ClaimsPrincipal?)null);

        var handler = new RefreshTokenHandler(NewDb(), jwtService);

        var result = await handler.ExecuteAsync(
            new RefreshTokenRequest { AccessToken = "invalid", RefreshToken = "token" },
            CancellationToken.None);

        Assert.NotNull(Assert.IsType<Result<RefreshTokenResponse>>(result).Problem);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnUnauthorized_WhenStoredRefreshTokenIsInvalid()
    {
        var userId = Guid.NewGuid();
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }));

        var jwtService = Substitute.For<IJwtService>();
        jwtService.GetPrincipalFromExpiredToken(Arg.Any<string>()).Returns(principal);

        var db = NewDb();
        var handler = new RefreshTokenHandler(db, jwtService);

        var result = await handler.ExecuteAsync(
            new RefreshTokenRequest { AccessToken = "access", RefreshToken = "refresh" },
            CancellationToken.None);

        Assert.NotNull(Assert.IsType<Result<RefreshTokenResponse>>(result).Problem);
        await db.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldIssueNewTokens_WhenRefreshTokenIsValid()
    {
        var user = User.Create("User", "user@example.com", "hash");
        var userId = user.Id;
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }));

        var jwtService = Substitute.For<IJwtService>();
        jwtService.GetPrincipalFromExpiredToken(Arg.Any<string>()).Returns(principal);

        var storedToken = RefreshToken.Create(userId, "stored-refresh", DateTime.UtcNow.AddDays(1));

        var db = NewDb(users: new[] { user }, tokens: new[] { storedToken });

        jwtService.GenerateAccessToken(user).Returns("new-access-token");
        var newRefreshToken = RefreshToken.Create(userId, "new-refresh-token", DateTime.UtcNow.AddDays(7));
        jwtService.GenerateRefreshToken(userId).Returns(newRefreshToken);

        var handler = new RefreshTokenHandler(db, jwtService);

        var result = await handler.ExecuteAsync(
            new RefreshTokenRequest { AccessToken = "access", RefreshToken = "stored-refresh" },
            CancellationToken.None);

        var response = Assert.IsType<Result<RefreshTokenResponse>>(result).Data;
        Assert.Equal("new-access-token", response.AccessToken);
        Assert.True(storedToken.IsRevoked);
        await db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
