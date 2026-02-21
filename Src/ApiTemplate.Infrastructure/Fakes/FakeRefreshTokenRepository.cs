using ApiTemplate.Application.Core.Entities;
using ApiTemplate.Application.Interfaces;

namespace ApiTemplate.Infrastructure.Fakes;

public class FakeRefreshTokenRepository : IRefreshTokenRepository
{
    private static readonly List<RefreshToken> Store = [];
    private static readonly object _lock = new();

    public Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var entity = Store.FirstOrDefault(rt => rt.Token == token);
            return Task.FromResult(entity);
        }
    }

    public Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            Store.Add(refreshToken);
            return Task.CompletedTask;
        }
    }

    public Task RevokeAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            refreshToken.Revoke();
            return Task.CompletedTask;
        }
    }
}