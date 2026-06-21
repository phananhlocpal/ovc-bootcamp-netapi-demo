namespace Demo.Contracts.Students;

public sealed record StudentResponse(
    Guid Id,
    string StudentCode,
    string FullName,
    string Email,
    DateOnly DateOfBirth,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
