using Demo.Authorization;
using Demo.Models.Auth;
using Microsoft.AspNetCore.Identity;

namespace Demo.Services.Users;

public sealed class DemoUserStore : IDemoUserStore
{
    private readonly IReadOnlyCollection<DemoUser> _users;

    public DemoUserStore()
    {
        var hasher = new PasswordHasher<DemoUser>();
        _users = CreateUsers(hasher);
    }

    public Task<DemoUser?> GetByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = _users.SingleOrDefault(x => x.Id == userId);
        return Task.FromResult(user);
    }

    public Task<DemoUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        var user = _users.SingleOrDefault(x => string.Equals(x.Username, username, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(user);
    }

    private static IReadOnlyCollection<DemoUser> CreateUsers(PasswordHasher<DemoUser> hasher)
    {
        var users = new[]
        {
            new DemoUser
            {
                Id = Guid.Parse("7c7d11bb-d9a4-4f13-b401-197412365001"),
                Username = "admin",
                PasswordHash = string.Empty,
                Roles = new[] { AppRoles.Admin, AppRoles.StudentReader, AppRoles.StudentWriter }
            },
            new DemoUser
            {
                Id = Guid.Parse("7c7d11bb-d9a4-4f13-b401-197412365002"),
                Username = "reader",
                PasswordHash = string.Empty,
                Roles = new[] { AppRoles.StudentReader }
            },
            new DemoUser
            {
                Id = Guid.Parse("7c7d11bb-d9a4-4f13-b401-197412365003"),
                Username = "writer",
                PasswordHash = string.Empty,
                Roles = new[] { AppRoles.StudentWriter }
            }
        };

        SetPassword(users[0], "Admin@123", hasher);
        SetPassword(users[1], "Reader@123", hasher);
        SetPassword(users[2], "Writer@123", hasher);

        return users;
    }

    private static void SetPassword(DemoUser user, string password, PasswordHasher<DemoUser> hasher)
    {
        user.PasswordHash = hasher.HashPassword(user, password);
    }
}
