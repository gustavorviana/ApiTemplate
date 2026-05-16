namespace ApiTemplate.Application.Interfaces;

public interface IAppDbContextFactory
{
    IAppDbContext Create();
}
