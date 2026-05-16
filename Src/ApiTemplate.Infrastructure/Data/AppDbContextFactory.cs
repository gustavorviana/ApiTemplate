using ApiTemplate.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ApiTemplate.Infrastructure.Data;

public class AppDbContextFactory(
    IDbContextFactory<AppDbContext> factory
#if (EnableJwt)
    , ICurrentUser currentUser
#endif
) : IAppDbContextFactory
{
    public IAppDbContext Create()
    {
        var context = factory.CreateDbContext();
#if (EnableJwt)
        context.CurrentUserId = currentUser.UserId;
#endif
        return context;
    }
}
