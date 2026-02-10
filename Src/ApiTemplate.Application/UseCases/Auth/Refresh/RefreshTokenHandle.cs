
using ApiTemplate.Application.Core.Helper;
using Microsoft.Extensions.Configuration;

namespace ApiTemplate.Application.UseCases.Auth.Refresh
{
    public class RefreshTokenHandle(IConfiguration configuration) : IUseCaseHandle<RefreshTokenRequest, AuthResponse>
    {
        private readonly JwtHelper _jwtHelper = new(configuration);
        public Task<AuthResponse> ExecuteAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
        {
            var user = _jwtHelper.ValidateRefreshToken(request);
            return Task.FromResult(new AuthResponse { AccessToken = _jwtHelper.GenerateToken(user), RefreshToken = _jwtHelper.GenerateAndSaveRefreshToken(user) });
        }
    }
}
