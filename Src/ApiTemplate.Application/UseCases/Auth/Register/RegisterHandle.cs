using Viana.Results;
using ApiTemplate.Application.Core.Entities;
using ApiTemplate.Application.Interfaces;
using ApiTemplate.Application.MessagesCatalog;

namespace ApiTemplate.Application.UseCases.Auth.Register;

public class RegisterHandle(
    IUserRepository userRepository,
    IPasswordSecurityProvider passwordSecurityProvider,
    IPasswordHasher passwordHasher) : IUseCaseHandle<RegisterRequest, Result<RegisterResponse>>
{
    public async Task<Result<RegisterResponse>> ExecuteAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        var existing = await userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (existing is not null)
            return new ProblemResult(409, Messages.Auth.UserWithEmailAlreadyExists);

        var strengthResult = passwordSecurityProvider.Evaluate(request.Password);

        if (!strengthResult.MeetsMinimum)
        {
            var message = string.IsNullOrWhiteSpace(strengthResult.MissingRequirements)
                ? Messages.Auth.PasswordMinimumRequirementsNotMet
                : strengthResult.MissingRequirements;

            return new ProblemResult(400, message);
        }

        var passwordHash = passwordHasher.Hash(request.Password);
        var user = User.Create(request.Name, request.Email, passwordHash);
        await userRepository.AddAsync(user, cancellationToken);

        var response = new RegisterResponse
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email
        };

        return new Result<RegisterResponse>(response, 201);
    }
}
