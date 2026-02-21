using ApiTemplate.Application.Core.Entities;
using ApiTemplate.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ApiTemplate.Infrastructure.Data.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IDbContext _context;

    public RefreshTokenRepository(IDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
        => await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);

    public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokeAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        refreshToken.Revoke();
        await _context.SaveChangesAsync(cancellationToken);
    }
}
