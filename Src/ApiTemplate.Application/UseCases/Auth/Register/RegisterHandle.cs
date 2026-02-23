using Viana.Results;
using ApiTemplate.Application.Core.Entities;
using ApiTemplate.Application.Interfaces;

namespace ApiTemplate.Application.UseCases.Auth.Register;

public class RegisterHandle : IUseCaseHandle<RegisterRequest, Result<RegisterResponse>>
{
    private readonly IUserRepository _userRepository;

    public RegisterHandle(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<RegisterResponse>> ExecuteAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        var existing = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (existing is not null)
        {
            return new ProblemResult(409, "User with this email already exists.");
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = User.Create(request.Name, request.Email, passwordHash);
        await _userRepository.AddAsync(user, cancellationToken);

        var response = new RegisterResponse
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email
        };

        return new Result<RegisterResponse>(response, 201);
    }
}
