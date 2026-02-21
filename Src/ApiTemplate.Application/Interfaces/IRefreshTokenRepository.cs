using ApiTemplate.Application.Core.Entities;

namespace ApiTemplate.Application.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task RevokeAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
}
