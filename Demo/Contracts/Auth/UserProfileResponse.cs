namespace Demo.Contracts.Auth;

public sealed record UserProfileResponse(Guid UserId, string Username, IReadOnlyCollection<string> Roles);
