namespace Demo.Models.Auth;

public sealed class RefreshTokenRecord
{
    public required string TokenHash { get; init; }
    public required Guid UserId { get; init; }
    public required string Username { get; init; }
    public required IReadOnlyCollection<string> Roles { get; init; }
    public required DateTime ExpiresAtUtc { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public required string FamilyId { get; init; }
    public DateTime? RevokedAtUtc { get; private set; }
    public string? ReplacedByTokenHash { get; private set; }

    public bool IsExpired(DateTime utcNow) => utcNow >= ExpiresAtUtc;

    public void Revoke(DateTime utcNow, string? replacedByTokenHash = null)
    {
        RevokedAtUtc ??= utcNow;
        ReplacedByTokenHash ??= replacedByTokenHash;
    }
}
