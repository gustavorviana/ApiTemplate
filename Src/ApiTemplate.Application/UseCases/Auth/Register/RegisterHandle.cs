#if (EnableResult)
using ApiTemplate.Application.Results;
#endif
using ApiTemplate.Application.Core.Entities;
using ApiTemplate.Application.Interfaces;

namespace ApiTemplate.Application.UseCases.Auth.Register;

#if (EnableResult)
public class RegisterHandle : IUseCaseHandle<RegisterRequest, Result<RegisterResponse>>
#else
public class RegisterHandle : IUseCaseHandle<RegisterRequest, RegisterResponse?>
#endif
{
    private readonly IUserRepository _userRepository;

    public RegisterHandle(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

#if (EnableResult)
    public async Task<Result<RegisterResponse>> ExecuteAsync(
#else
    public async Task<RegisterResponse?> ExecuteAsync(
#endif
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        var existing = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (existing is not null)
        {
#if (EnableResult)
            return new ProblemResult(409, "User with this email already exists.");
#else
            return null;
#endif
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

#if (EnableResult)
        return new Result<RegisterResponse>(201, response);
#else
        return response;
#endif
    }
}
