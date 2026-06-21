using Demo.Models.Auth;

namespace Demo.Services.Tokens;

public interface IRefreshTokenStore
{
    Task StoreAsync(RefreshTokenRecord token, CancellationToken cancellationToken);
    Task<RefreshTokenRecord?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken);
    Task RotateAsync(string currentTokenHash, RefreshTokenRecord replacementToken, DateTime revokedAtUtc, CancellationToken cancellationToken);
    Task RevokeAsync(string tokenHash, DateTime revokedAtUtc, CancellationToken cancellationToken);
    Task RevokeFamilyAsync(Guid userId, string familyId, DateTime revokedAtUtc, CancellationToken cancellationToken);
}
