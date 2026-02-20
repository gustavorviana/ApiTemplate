using ApiTemplate.Application.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiTemplate.Application.Interfaces;

public interface IDbContext
{
    DbSet<User> Users { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
