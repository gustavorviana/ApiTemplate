using Viana.Results;
using ApiTemplate.Application.Core.Entities;
using ApiTemplate.Application.Interfaces;
using ApiTemplate.Application.MessagesCatalog;
using Microsoft.EntityFrameworkCore;

namespace ApiTemplate.Application.UseCases.Auth.Register;

public class RegisterHandler(
    IDbContext db,
#if (EnablePasswordSecurity)
    IPasswordSecurityProvider passwordSecurityProvider,
#endif
    IPasswordHasher passwordHasher) : IUseCaseHandler<RegisterRequest, Result<RegisterResponse>>
{
    public async Task<Result<RegisterResponse>> ExecuteAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        var existing = await db.Users
            .AnyAsync(u => u.Email == request.Email, cancellationToken);

        if (existing)
            return new ProblemResult(409, Messages.Auth.UserWithEmailAlreadyExists);

#if (EnablePasswordSecurity)
        var strengthResult = passwordSecurityProvider.Evaluate(request.Password);

        if (!strengthResult.MeetsMinimum)
        {
            var message = string.IsNullOrWhiteSpace(strengthResult.MissingRequirements)
                ? Messages.Auth.PasswordMinimumRequirementsNotMet
                : strengthResult.MissingRequirements;

            return new ProblemResult(400, message);
        }
#endif

        var passwordHash = passwordHasher.Hash(request.Password);
        var user = User.Create(request.Name, request.Email, passwordHash);

        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);

        var response = new RegisterResponse
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email
        };

        return new Result<RegisterResponse>(response, 201);
    }
}
