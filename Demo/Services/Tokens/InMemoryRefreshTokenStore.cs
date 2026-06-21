using System.Collections.Concurrent;
using Demo.Models.Auth;

namespace Demo.Services.Tokens;

public sealed class InMemoryRefreshTokenStore : IRefreshTokenStore
{
    private readonly ConcurrentDictionary<string, RefreshTokenRecord> _tokens = new();

    public Task StoreAsync(RefreshTokenRecord token, CancellationToken cancellationToken)
    {
        _tokens[token.TokenHash] = token;
        return Task.CompletedTask;
    }

    public Task<RefreshTokenRecord?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken)
    {
        _tokens.TryGetValue(tokenHash, out var token);
        return Task.FromResult(token);
    }

    public Task RotateAsync(string currentTokenHash, RefreshTokenRecord replacementToken, DateTime revokedAtUtc, CancellationToken cancellationToken)
    {
        if (_tokens.TryGetValue(currentTokenHash, out var currentToken))
        {
            currentToken.Revoke(revokedAtUtc, replacementToken.TokenHash);
        }

        _tokens[replacementToken.TokenHash] = replacementToken;
        return Task.CompletedTask;
    }

    public Task RevokeAsync(string tokenHash, DateTime revokedAtUtc, CancellationToken cancellationToken)
    {
        if (_tokens.TryGetValue(tokenHash, out var token))
        {
            token.Revoke(revokedAtUtc);
        }

        return Task.CompletedTask;
    }

    public Task RevokeFamilyAsync(Guid userId, string familyId, DateTime revokedAtUtc, CancellationToken cancellationToken)
    {
        var tokens = _tokens.Values.Where(token => token.UserId == userId && token.FamilyId == familyId);

        foreach (var token in tokens)
        {
            token.Revoke(revokedAtUtc);
        }

        return Task.CompletedTask;
    }
}
