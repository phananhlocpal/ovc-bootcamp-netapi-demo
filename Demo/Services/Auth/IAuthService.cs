using Demo.Contracts.Auth;

namespace Demo.Services.Auth;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<LoginResponse> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken);
    Task RevokeAsync(Guid userId, RevokeRefreshTokenRequest request, CancellationToken cancellationToken);
    Task<UserProfileResponse> GetProfileAsync(Guid userId, CancellationToken cancellationToken);
}
