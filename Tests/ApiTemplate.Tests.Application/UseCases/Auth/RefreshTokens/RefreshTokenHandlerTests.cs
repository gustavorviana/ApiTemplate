using Viana.Results;
using ApiTemplate.Application.Core.Entities;
using ApiTemplate.Application.Core.ValueObjects;
using ApiTemplate.Application.Interfaces;
using ApiTemplate.Application.UseCases.Auth.RefreshToken;
using ApiTemplate.Tests.Application.Base;
using NSubstitute;

namespace ApiTemplate.Tests.Application.UseCases.Auth.RefreshTokens;

public class RefreshTokenHandlerTests : TestBase
{
    private readonly IJwtService _jwtService = Substitute.For<IJwtService>();
    private readonly RefreshTokenHandler _handler;

    public RefreshTokenHandlerTests()
    {
        _handler = new RefreshTokenHandler(AppDbContextFactory, _jwtService);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnUnauthorized_WhenRefreshTokenDoesNotExist()
    {
        var usersDbSet = ToMockDbSet(Array.Empty<UserEntity>());
        var tokensDbSet = ToMockDbSet(Array.Empty<RefreshTokenEntity>());
        AppContext.Users.Returns(usersDbSet);
        AppContext.RefreshTokens.Returns(tokensDbSet);

        var result = await _handler.ExecuteAsync(
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
        var usersDbSet = ToMockDbSet(Array.Empty<UserEntity>());
        var tokensDbSet = ToMockDbSet(new[] { revoked });
        AppContext.Users.Returns(usersDbSet);
        AppContext.RefreshTokens.Returns(tokensDbSet);

        var result = await _handler.ExecuteAsync(
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
        var usersDbSet = ToMockDbSet(new[] { user });
        var tokensDbSet = ToMockDbSet(new[] { storedToken });
        AppContext.Users.Returns(usersDbSet);
        AppContext.RefreshTokens.Returns(tokensDbSet);

        _jwtService.GenerateAccessToken(Arg.Any<JwtClaimsContext>()).Returns("new-access-token");
        var newRefreshToken = new RefreshTokenEntity
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = "new-refresh-token",
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        _jwtService.GenerateRefreshToken(user.Id).Returns(newRefreshToken);

        var result = await _handler.ExecuteAsync(
            new RefreshTokenRequest { RefreshToken = storedToken.Token },
            CancellationToken.None);

        var response = Assert.IsType<Result<RefreshTokenResponse>>(result).Data;
        Assert.Equal("new-access-token", response.AccessToken);
        Assert.True(storedToken.IsRevoked);
    }
}
