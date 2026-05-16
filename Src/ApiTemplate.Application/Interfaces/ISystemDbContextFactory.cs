namespace ApiTemplate.Application.Interfaces;

/// <summary>
/// Creates an <see cref="IAppDbContext"/> without binding to any current user.
/// Use this in background jobs, migrations, seed routines and startup tasks —
/// anywhere there is no HTTP request and therefore no <c>ICurrentUser</c>.
/// </summary>
public interface ISystemDbContextFactory
{
    IAppDbContext Create();
}
