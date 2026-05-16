using Viana.Results;
using ApiTemplate.Application.Core.Entities;
using ApiTemplate.Application.Interfaces;
using ApiTemplate.Application.UseCases.Auth.Register;
using ApiTemplate.Tests.Application.Base;
using NSubstitute;
#if (EnablePasswordSecurity)
using ApiTemplate.Application.Core.Enums;
using ApiTemplate.Application.Core.ValueObjects;
#endif

namespace ApiTemplate.Tests.Application.UseCases.Auth.Register;

public class RegisterHandlerTests : TestBase
{
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
#if (EnablePasswordSecurity)
    private readonly IPasswordSecurityProvider _passwordSecurity = Substitute.For<IPasswordSecurityProvider>();
#endif
    private readonly RegisterHandler _handler;

    public RegisterHandlerTests()
    {
#if (EnablePasswordSecurity)
        _passwordSecurity
            .Evaluate(Arg.Any<string>())
            .Returns(_ => new PasswordStrengthResult
            {
                Strength = PasswordStrengthStatus.Strong,
                MinimumRequired = PasswordStrengthStatus.Medium
            });
#endif

        _handler = new RegisterHandler(
            AppDbContextFactory,
#if (EnablePasswordSecurity)
            _passwordSecurity,
#endif
            _passwordHasher);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnConflict_WhenUserWithEmailAlreadyExists()
    {
        var existing = new UserEntity
        {
            Id = Guid.NewGuid(),
            Name = "Existing",
            Email = "user@example.com",
            PasswordHash = "hash"
        };
        var dbSet = ToMockDbSet(new[] { existing });
        AppContext.Users.Returns(dbSet);

        var result = await _handler.ExecuteAsync(
            new RegisterRequest { Name = "New", Email = existing.Email, Password = "password" },
            CancellationToken.None);

        Assert.NotNull(Assert.IsType<Result<RegisterResponse>>(result).Problem);
        await AppContext.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCreateUser_WhenEmailDoesNotExist()
    {
        var store = new List<UserEntity>();
        var usersSet = ToMockDbSet(store);
        usersSet
            .When(s => s.Add(Arg.Any<UserEntity>()))
            .Do(ci => store.Add(ci.Arg<UserEntity>()));
        AppContext.Users.Returns(usersSet);

        _passwordHasher.Hash(Arg.Any<string>()).Returns("hashed-password");

        var result = await _handler.ExecuteAsync(
            new RegisterRequest { Name = "New User", Email = "user@example.com", Password = "password" },
            CancellationToken.None);

        await AppContext.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        Assert.Single(store);
        Assert.Equal("hashed-password", store[0].PasswordHash);
        Assert.IsType<Result<RegisterResponse>>(result);
    }
}
