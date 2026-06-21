using Demo.Models.Auth;

namespace Demo.Services.Users;

public interface IDemoUserStore
{
    Task<DemoUser?> GetByIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<DemoUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken);
}
