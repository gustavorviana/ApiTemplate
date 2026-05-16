using ApiTemplate.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace ApiTemplate.Tests.Jobs.Base;

public abstract class TestBase
{
    protected IAppDbContext AppContext { get; set; }
    protected IAppDbContextFactory AppDbContextFactory { get; set; }

    protected TestBase()
    {
        AppContext = Substitute.For<IAppDbContext>();
        AppDbContextFactory = Substitute.For<IAppDbContextFactory>();
        AppDbContextFactory.Create().Returns(AppContext);
    }

    protected static DbSet<T> ToMockDbSet<T>(ICollection<T> data) where T : class
        => data.BuildMockDbSet();
}
