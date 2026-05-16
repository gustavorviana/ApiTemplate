using ApiTemplate.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace ApiTemplate.Tests.Jobs.Base;

public abstract class TestBase
{
    protected IAppDbContext AppContext { get; set; }
    protected ISystemDbContextFactory SystemDbContextFactory { get; set; }

    protected TestBase()
    {
        AppContext = Substitute.For<IAppDbContext>();
        SystemDbContextFactory = Substitute.For<ISystemDbContextFactory>();
        SystemDbContextFactory.Create().Returns(AppContext);
    }

    protected static DbSet<T> ToMockDbSet<T>(ICollection<T> data) where T : class
        => data.BuildMockDbSet();
}
