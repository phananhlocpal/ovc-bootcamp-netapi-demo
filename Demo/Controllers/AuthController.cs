using System.Security.Claims;
using Demo.Contracts.Auth;
using Demo.Exceptions;
using Demo.Options;
using Demo.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Demo.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(
    IAuthService authService,
    IOptions<RefreshTokenCookieOptions> refreshTokenCookieOptions) : ControllerBase
{
    private readonly RefreshTokenCookieOptions _refreshTokenCookieOptions = refreshTokenCookieOptions.Value;

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var tokens = await authService.LoginAsync(request, cancellationToken);
        WriteRefreshTokenCookie(tokens.RefreshToken, tokens.RefreshTokenExpiresAtUtc);
        return Ok(MapToResponse(tokens));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<LoginResponse>> Refresh(CancellationToken cancellationToken)
    {
        var refreshToken = GetRefreshTokenFromCookie();
        var tokens = await authService.RefreshAsync(refreshToken, cancellationToken);
        WriteRefreshTokenCookie(tokens.RefreshToken, tokens.RefreshTokenExpiresAtUtc);
        return Ok(MapToResponse(tokens));
    }

    [HttpPost("revoke")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Revoke(CancellationToken cancellationToken)
    {
        var userId = GetRequiredUserId();
        var refreshToken = GetRefreshTokenFromCookie();
        await authService.RevokeAsync(userId, refreshToken, cancellationToken);
        DeleteRefreshTokenCookie();
        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserProfileResponse>> Me(CancellationToken cancellationToken)
    {
        var userId = GetRequiredUserId();
        var response = await authService.GetProfileAsync(userId, cancellationToken);
        return Ok(response);
    }

    private Guid GetRequiredUserId()
    {
        var rawValue = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Authenticated user id is missing.");

        return Guid.Parse(rawValue);
    }

    private string GetRefreshTokenFromCookie()
    {
        if (Request.Cookies.TryGetValue(_refreshTokenCookieOptions.Name, out var refreshToken) &&
            !string.IsNullOrWhiteSpace(refreshToken))
        {
            return refreshToken;
        }

        throw new UnauthorizedAppException("Refresh token cookie is missing.");
    }

    private void WriteRefreshTokenCookie(string refreshToken, DateTime expiresAtUtc)
    {
        Response.Cookies.Append(_refreshTokenCookieOptions.Name, refreshToken, new CookieOptions
        {
            HttpOnly = _refreshTokenCookieOptions.HttpOnly,
            Secure = _refreshTokenCookieOptions.Secure,
            SameSite = _refreshTokenCookieOptions.SameSite,
            Path = _refreshTokenCookieOptions.Path,
            Expires = expiresAtUtc
        });
    }

    private void DeleteRefreshTokenCookie()
    {
        Response.Cookies.Delete(_refreshTokenCookieOptions.Name, new CookieOptions
        {
            HttpOnly = _refreshTokenCookieOptions.HttpOnly,
            Secure = _refreshTokenCookieOptions.Secure,
            SameSite = _refreshTokenCookieOptions.SameSite,
            Path = _refreshTokenCookieOptions.Path
        });
    }

    private static LoginResponse MapToResponse(AuthTokensResult tokens)
    {
        return new LoginResponse(
            tokens.AccessToken,
            tokens.AccessTokenExpiresAtUtc,
            tokens.RefreshTokenExpiresAtUtc,
            tokens.TokenType,
            tokens.Roles);
    }
}
