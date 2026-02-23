using Viana.Results;
using ApiTemplate.Application.Core.Entities;
using ApiTemplate.Application.Interfaces;
using ApiTemplate.Application.UseCases.Auth.Register;
using NSubstitute;

namespace ApiTemplate.Tests.Application.UseCases.Auth.Register;

public class RegisterHandleTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldReturnConflict_WhenUserWithEmailAlreadyExists()
    {
        var existingUser = User.Create("Existing", "user@example.com", "hash");

        var userRepository = Substitute.For<IUserRepository>();
        userRepository
            .GetByEmailAsync(existingUser.Email, Arg.Any<CancellationToken>())
            .Returns(existingUser);

        var handle = new RegisterHandle(userRepository);
        var request = new RegisterRequest
        {
            Name = "New User",
            Email = existingUser.Email,
            Password = "password"
        };

        var result = await handle.ExecuteAsync(request, CancellationToken.None);

        var typedResult = Assert.IsType<Result<RegisterResponse>>(result);
        Assert.NotNull(typedResult.Problem);

        await userRepository
            .DidNotReceive()
            .AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCreateUser_WhenEmailDoesNotExist()
    {
        var userRepository = Substitute.For<IUserRepository>();
        userRepository
            .GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        User? addedUser = null;
        await userRepository
            .AddAsync(
                Arg.Do<User>(u => addedUser = u),
                Arg.Any<CancellationToken>());

        var handle = new RegisterHandle(userRepository);
        var request = new RegisterRequest
        {
            Name = "New User",
            Email = "user@example.com",
            Password = "password"
        };

        var result = await handle.ExecuteAsync(request, CancellationToken.None);

        await userRepository
            .Received(1)
            .AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());

        Assert.NotNull(addedUser);
        Assert.Equal(request.Name, addedUser!.Name);
        Assert.Equal(request.Email, addedUser.Email);

        var response = Assert.IsType<Result<RegisterResponse>>(result).Data!;
        Assert.Equal(addedUser.Id, response.Id);
        Assert.Equal(request.Name, response.Name);
        Assert.Equal(request.Email, response.Email);
    }
}
