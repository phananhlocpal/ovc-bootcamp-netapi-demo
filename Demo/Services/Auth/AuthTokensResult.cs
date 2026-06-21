namespace Demo.Services.Auth;

public sealed record AuthTokensResult(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc,
    string TokenType,
    IReadOnlyCollection<string> Roles);
