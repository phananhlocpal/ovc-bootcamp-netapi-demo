namespace Demo.Models.Auth;

public sealed class DemoUser
{
    public required Guid Id { get; init; }
    public required string Username { get; init; }
    public required string PasswordHash { get; set; }
    public required IReadOnlyCollection<string> Roles { get; init; }
}
