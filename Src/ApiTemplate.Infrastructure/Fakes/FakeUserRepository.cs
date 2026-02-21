using ApiTemplate.Application.Core.Entities;
using ApiTemplate.Application.Interfaces;

namespace ApiTemplate.Infrastructure.Fakes;

public class FakeUserRepository : IUserRepository
{
    private static readonly List<User> Store;

    static FakeUserRepository()
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!");
        var admin = User.Create("Admin", "admin@admin.com", passwordHash);
        Store = [admin];
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(Store.FirstOrDefault(u => u.Id == id));

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => Task.FromResult(Store.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)));

    public Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<User>>(Store.ToList());

    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        Store.Add(user);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(User user, CancellationToken cancellationToken = default)
    {
        Store.RemoveAll(u => u.Id == user.Id);
        return Task.CompletedTask;
    }
}