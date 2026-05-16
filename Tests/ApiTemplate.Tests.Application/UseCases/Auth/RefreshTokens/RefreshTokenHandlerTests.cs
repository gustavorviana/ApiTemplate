using Viana.Results;
using ApiTemplate.Application.Core.Entities;
using ApiTemplate.Application.Core.ValueObjects;
using ApiTemplate.Application.Interfaces;
using ApiTemplate.Application.UseCases.Auth.RefreshToken;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace ApiTemplate.Tests.Application.UseCases.Auth.RefreshTokens;

public class RefreshTokenHandlerTests
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

    [Fact]
    public async Task ExecuteAsync_ShouldReturnUnauthorized_WhenRefreshTokenDoesNotExist()
    {
        var factory = NewFactory();
        var handler = new RefreshTokenHandler(factory, Substitute.For<IJwtService>());

        var result = await handler.ExecuteAsync(
            new RefreshTokenRequest { RefreshToken = "missing" },
            CancellationToken.None);

        Assert.NotNull(Assert.IsType<Result<RefreshTokenResponse>>(result).Problem);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnUnauthorized_WhenRefreshTokenIsRevoked()
    {
        var revoked = new RefreshTokenEntity
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Token = "revoked-token",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            IsRevoked = true
        };

        var factory = NewFactory(tokens: new[] { revoked });
        var handler = new RefreshTokenHandler(factory, Substitute.For<IJwtService>());

        var result = await handler.ExecuteAsync(
            new RefreshTokenRequest { RefreshToken = revoked.Token },
            CancellationToken.None);

        Assert.NotNull(Assert.IsType<Result<RefreshTokenResponse>>(result).Problem);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldIssueNewTokens_WhenRefreshTokenIsValid()
    {
        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            Name = "User",
            Email = "user@example.com",
            PasswordHash = "hash"
        };

        var storedToken = new RefreshTokenEntity
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = "stored-refresh",
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };

        var factory = NewFactory(users: new[] { user }, tokens: new[] { storedToken });

        var jwtService = Substitute.For<IJwtService>();
        jwtService.GenerateAccessToken(Arg.Any<JwtClaimsContext>()).Returns("new-access-token");

        var newRefreshToken = new RefreshTokenEntity
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = "new-refresh-token",
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        jwtService.GenerateRefreshToken(user.Id).Returns(newRefreshToken);

        var handler = new RefreshTokenHandler(factory, jwtService);

        var result = await handler.ExecuteAsync(
            new RefreshTokenRequest { RefreshToken = storedToken.Token },
            CancellationToken.None);

        var response = Assert.IsType<Result<RefreshTokenResponse>>(result).Data;
        Assert.Equal("new-access-token", response.AccessToken);
        Assert.True(storedToken.IsRevoked);
    }
}
