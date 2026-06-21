namespace Demo.Contracts.Auth;

public sealed record LoginResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc,
    string TokenType,
    IReadOnlyCollection<string> Roles);
