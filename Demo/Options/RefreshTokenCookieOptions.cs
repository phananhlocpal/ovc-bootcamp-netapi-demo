namespace Demo.Options;

public sealed class RefreshTokenCookieOptions
{
    public const string SectionName = "RefreshTokenCookie";

    public string Name { get; init; } = "demo_refresh_token";
    public string Path { get; init; } = "/api/auth";
    public bool HttpOnly { get; init; } = true;
    public bool Secure { get; init; } = true;
    public SameSiteMode SameSite { get; init; } = SameSiteMode.Strict;
}
