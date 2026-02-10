using ApiTemplate.Application.Core.Entities;
using ApiTemplate.Application.Core.Exceptions;
using ApiTemplate.Application.Core.Helper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace ApiTemplate.Application.UseCases.Auth.Login
{
    public class LoginHandle(IConfiguration configuration) : IUseCaseHandle<AuthRequest, AuthResponse>
    {
        private readonly JwtHelper _jwtHelper = new(configuration);

        public Task<AuthResponse> ExecuteAsync(AuthRequest request, CancellationToken cancellationToken = default)
        {
            var user = JwtHelper.user; // In a real application, you would retrieve the user from the database based on the email provided in the request.

            if (user.Email != request.Email || new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
                throw new BadRequestException("User or password is incorrect");

            return Task.FromResult(new AuthResponse { AccessToken = _jwtHelper.GenerateToken(user), RefreshToken = _jwtHelper.GenerateAndSaveRefreshToken(user) });
        }
    }
}
