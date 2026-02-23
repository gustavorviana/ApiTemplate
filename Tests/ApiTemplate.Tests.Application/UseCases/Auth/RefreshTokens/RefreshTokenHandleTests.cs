using Viana.Results;
using System.Security.Claims;
using ApiTemplate.Application.Core.Entities;
using ApiTemplate.Application.Interfaces;
using ApiTemplate.Application.UseCases.Auth.RefreshToken;
using NSubstitute;

namespace ApiTemplate.Tests.Application.UseCases.Auth.RefreshTokens;

public class RefreshTokenHandleTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldReturnUnauthorized_WhenAccessTokenIsInvalid()
    {
        var jwtService = Substitute.For<IJwtService>();
        jwtService
            .GetPrincipalFromExpiredToken(Arg.Any<string>())
            .Returns((ClaimsPrincipal?)null);

        var refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
        var userRepository = Substitute.For<IUserRepository>();

        var handle = new RefreshTokenHandle(
            jwtService,
            refreshTokenRepository,
            userRepository);

        var request = new RefreshTokenRequest
        {
            AccessToken = "invalid",
            RefreshToken = "token"
        };

        var result = await handle.ExecuteAsync(request, CancellationToken.None);

        var typedResult = Assert.IsType<Result<RefreshTokenResponse>>(result);
        Assert.NotNull(typedResult.Problem);

        await refreshTokenRepository
            .DidNotReceive()
            .GetByTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnUnauthorized_WhenStoredRefreshTokenIsInvalid()
    {
        var userId = Guid.NewGuid();

        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }));

        var jwtService = Substitute.For<IJwtService>();
        jwtService
            .GetPrincipalFromExpiredToken(Arg.Any<string>())
            .Returns(principal);

        var refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
        refreshTokenRepository
            .GetByTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((RefreshToken?)null);

        var userRepository = Substitute.For<IUserRepository>();

        var handle = new RefreshTokenHandle(
            jwtService,
            refreshTokenRepository,
            userRepository);

        var request = new RefreshTokenRequest
        {
            AccessToken = "access",
            RefreshToken = "refresh"
        };

        var result = await handle.ExecuteAsync(request, CancellationToken.None);

        var typedResult = Assert.IsType<Result<RefreshTokenResponse>>(result);
        Assert.NotNull(typedResult.Problem);

        await refreshTokenRepository
            .DidNotReceive()
            .RevokeAsync(Arg.Any<RefreshToken>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldIssueNewTokens_WhenRefreshTokenIsValid()
    {
        var userId = Guid.NewGuid();

        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }));

        var jwtService = Substitute.For<IJwtService>();
        jwtService
            .GetPrincipalFromExpiredToken(Arg.Any<string>())
            .Returns(principal);

        var storedToken = RefreshToken.Create(
            userId,
            "stored-refresh",
            DateTime.UtcNow.AddDays(1));

        var refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
        refreshTokenRepository
            .GetByTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(storedToken);

        var user = User.Create("User", "user@example.com", "hash");
        var userRepository = Substitute.For<IUserRepository>();
        userRepository
            .GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        jwtService
            .GenerateAccessToken(user)
            .Returns("new-access-token");

        var newRefreshToken = RefreshToken.Create(
            userId,
            "new-refresh-token",
            DateTime.UtcNow.AddDays(7));

        jwtService
            .GenerateRefreshToken(userId)
            .Returns(newRefreshToken);

        var handle = new RefreshTokenHandle(
            jwtService,
            refreshTokenRepository,
            userRepository);

        var request = new RefreshTokenRequest
        {
            AccessToken = "access",
            RefreshToken = "stored-refresh"
        };

        var result = await handle.ExecuteAsync(request, CancellationToken.None);

        var response = Assert.IsType<Result<RefreshTokenResponse>>(result).Data;
        Assert.Equal("new-access-token", response.AccessToken);
        Assert.Equal("new-refresh-token", response.RefreshToken);

        await refreshTokenRepository
            .Received(1)
            .RevokeAsync(storedToken, Arg.Any<CancellationToken>());

        await refreshTokenRepository
            .Received(1)
            .AddAsync(newRefreshToken, Arg.Any<CancellationToken>());
    }
}
