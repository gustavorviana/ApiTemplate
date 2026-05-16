using ApiTemplate.Application.Core.Entities;
using ApiTemplate.Application.Interfaces;
using ApiTemplate.Application.Resources;
using Microsoft.EntityFrameworkCore;
using Viana.Results;

namespace ApiTemplate.Application.UseCases.Auth.Register;

public class RegisterHandler(
    IAppDbContextFactory dbFactory,
#if (EnablePasswordSecurity)
    IPasswordSecurityProvider passwordSecurityProvider,
#endif
    IPasswordHasher passwordHasher) : IUseCaseHandler<RegisterRequest, Result<RegisterResponse>>
{
    public async Task<Result<RegisterResponse>> ExecuteAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        await using var db = dbFactory.Create();

        var email = request.Email.Trim().ToLowerInvariant();
        var existing = await db.Users
            .AnyAsync(u => u.Email == email, cancellationToken);

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
        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Email = email,
            PasswordHash = passwordHash
        };

        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);

        return new Result<RegisterResponse>(new RegisterResponse
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email
        }, 201);
    }
}
