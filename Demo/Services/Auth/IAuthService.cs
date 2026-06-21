using Demo.Contracts.Auth;

namespace Demo.Services.Auth;

public interface IAuthService
{
    Task<AuthTokensResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<AuthTokensResult> RefreshAsync(string refreshToken, CancellationToken cancellationToken);
    Task RevokeAsync(Guid userId, string refreshToken, CancellationToken cancellationToken);
    Task<UserProfileResponse> GetProfileAsync(Guid userId, CancellationToken cancellationToken);
}
