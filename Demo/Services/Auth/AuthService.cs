using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Demo.Contracts.Auth;
using Demo.Exceptions;
using Demo.Models.Auth;
using Demo.Options;
using Demo.Services.Tokens;
using Demo.Services.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Demo.Services.Auth;

public sealed class AuthService(
    IDemoUserStore userStore,
    IRefreshTokenStore refreshTokenStore,
    IOptions<JwtOptions> jwtOptions,
    ILogger<AuthService> logger) : IAuthService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;
    private readonly PasswordHasher<DemoUser> _passwordHasher = new();

    public async Task<UserProfileResponse> GetProfileAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userStore.GetByIdAsync(userId, cancellationToken)
            ?? throw new UnauthorizedAppException("User was not found.");

        return new UserProfileResponse(user.Id, user.Username, user.Roles);
    }

    public async Task<AuthTokensResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await userStore.GetByUsernameAsync(request.Username, cancellationToken)
            ?? throw new UnauthorizedAppException("Invalid username or password.");

        var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAppException("Invalid username or password.");
        }

        logger.LogInformation("User {Username} logged in successfully.", user.Username);
        return await IssueTokensAsync(user, replacedToken: null, cancellationToken);
    }

    public async Task<AuthTokensResult> RefreshAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var presentedTokenHash = ComputeSha256(refreshToken);
        var existingToken = await refreshTokenStore.GetByHashAsync(presentedTokenHash, cancellationToken)
            ?? throw new UnauthorizedAppException("Refresh token is invalid.");

        if (existingToken.RevokedAtUtc is not null)
        {
            await refreshTokenStore.RevokeFamilyAsync(existingToken.UserId, existingToken.FamilyId, DateTime.UtcNow, cancellationToken);
            throw new UnauthorizedAppException("Refresh token has already been used or revoked.");
        }

        if (existingToken.IsExpired(DateTime.UtcNow))
        {
            throw new UnauthorizedAppException("Refresh token has expired.");
        }

        var user = await userStore.GetByIdAsync(existingToken.UserId, cancellationToken)
            ?? throw new UnauthorizedAppException("User was not found.");

        return await IssueTokensAsync(user, existingToken, cancellationToken);
    }

    public async Task RevokeAsync(Guid userId, string refreshToken, CancellationToken cancellationToken)
    {
        var tokenHash = ComputeSha256(refreshToken);
        var token = await refreshTokenStore.GetByHashAsync(tokenHash, cancellationToken)
            ?? throw new UnauthorizedAppException("Refresh token is invalid.");

        if (token.UserId != userId)
        {
            throw new UnauthorizedAppException("Refresh token does not belong to the authenticated user.");
        }

        await refreshTokenStore.RevokeAsync(tokenHash, DateTime.UtcNow, cancellationToken);
        logger.LogInformation("Refresh token revoked for user {UserId}.", userId);
    }

    private async Task<AuthTokensResult> IssueTokensAsync(
        DemoUser user,
        RefreshTokenRecord? replacedToken,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var accessTokenExpiresAtUtc = now.AddMinutes(_jwtOptions.AccessTokenMinutes);
        var refreshTokenExpiresAtUtc = now.AddDays(_jwtOptions.RefreshTokenDays);
        var accessToken = GenerateAccessToken(user, accessTokenExpiresAtUtc);
        var refreshToken = GenerateSecureToken();
        var refreshTokenHash = ComputeSha256(refreshToken);

        var refreshTokenRecord = new RefreshTokenRecord
        {
            TokenHash = refreshTokenHash,
            UserId = user.Id,
            Username = user.Username,
            Roles = user.Roles,
            CreatedAtUtc = now,
            ExpiresAtUtc = refreshTokenExpiresAtUtc,
            FamilyId = replacedToken?.FamilyId ?? Guid.NewGuid().ToString("N")
        };

        if (replacedToken is not null)
        {
            await refreshTokenStore.RotateAsync(replacedToken.TokenHash, refreshTokenRecord, now, cancellationToken);
        }
        else
        {
            await refreshTokenStore.StoreAsync(refreshTokenRecord, cancellationToken);
        }

        return new AuthTokensResult(
            accessToken,
            accessTokenExpiresAtUtc,
            refreshToken,
            refreshTokenExpiresAtUtc,
            "Bearer",
            user.Roles);
    }

    private string GenerateAccessToken(DemoUser user, DateTime expiresAtUtc)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };

        claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateSecureToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    private static string ComputeSha256(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }
}
