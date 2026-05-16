using Viana.Results;
using ApiTemplate.Application.Core.Entities;
using ApiTemplate.Application.Interfaces;
using ApiTemplate.Application.UseCases.Auth.Register;
using MockQueryable.NSubstitute;
using NSubstitute;
#if (EnablePasswordSecurity)
using ApiTemplate.Application.Core.Enums;
using ApiTemplate.Application.Core.ValueObjects;
#endif

namespace ApiTemplate.Tests.Application.UseCases.Auth.Register;

public class RegisterHandlerTests
{
#if (EnablePasswordSecurity)
    private static IPasswordSecurityProvider NewPasswordSecurity()
    {
        var provider = Substitute.For<IPasswordSecurityProvider>();
        provider
            .Evaluate(Arg.Any<string>())
            .Returns(_ => new PasswordStrengthResult
            {
                Strength = PasswordStrengthStatus.Strong,
                MinimumRequired = PasswordStrengthStatus.Medium
            });
        return provider;
    }
#endif

    private static (IAppDbContextFactory factory, IAppDbContext db) NewFactory(IEnumerable<UserEntity>? users = null)
    {
        var dbSet = (users ?? new List<UserEntity>()).ToList().BuildMockDbSet();
        var db = Substitute.For<IAppDbContext>();
        db.Users.Returns(dbSet);

        var factory = Substitute.For<IAppDbContextFactory>();
        factory.Create().Returns(db);
        return (factory, db);
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
        var (factory, db) = NewFactory(new[] { existing });

        var handler = new RegisterHandler(
            factory,
#if (EnablePasswordSecurity)
            NewPasswordSecurity(),
#endif
            Substitute.For<IPasswordHasher>());

        var result = await handler.ExecuteAsync(
            new RegisterRequest { Name = "New", Email = existing.Email, Password = "password" },
            CancellationToken.None);

        Assert.NotNull(Assert.IsType<Result<RegisterResponse>>(result).Problem);
        await db.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCreateUser_WhenEmailDoesNotExist()
    {
        var store = new List<UserEntity>();
        var usersSet = store.BuildMockDbSet();
        usersSet
            .When(s => s.Add(Arg.Any<UserEntity>()))
            .Do(ci => store.Add(ci.Arg<UserEntity>()));

        var db = Substitute.For<IAppDbContext>();
        db.Users.Returns(usersSet);

        var factory = Substitute.For<IAppDbContextFactory>();
        factory.Create().Returns(db);

        var passwordHasher = Substitute.For<IPasswordHasher>();
        passwordHasher.Hash(Arg.Any<string>()).Returns("hashed-password");

        var handler = new RegisterHandler(
            factory,
#if (EnablePasswordSecurity)
            NewPasswordSecurity(),
#endif
            passwordHasher);

        var result = await handler.ExecuteAsync(
            new RegisterRequest { Name = "New User", Email = "user@example.com", Password = "password" },
            CancellationToken.None);

        await db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        Assert.Single(store);
        Assert.Equal("hashed-password", store[0].PasswordHash);
        Assert.IsType<Result<RegisterResponse>>(result);
    }
}
