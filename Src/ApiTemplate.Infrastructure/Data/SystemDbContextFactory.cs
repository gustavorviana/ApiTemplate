using ApiTemplate.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ApiTemplate.Infrastructure.Data;

public class SystemDbContextFactory(IDbContextFactory<AppDbContext> factory) : ISystemDbContextFactory
{
    public IAppDbContext Create() => factory.CreateDbContext();
}
