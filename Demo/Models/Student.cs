namespace Demo.Models;

public sealed class Student
{
    public required Guid Id { get; init; }
    public required string StudentCode { get; set; }
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required DateOnly DateOfBirth { get; set; }
    public required DateTime CreatedAtUtc { get; init; }
    public required DateTime UpdatedAtUtc { get; set; }
}
