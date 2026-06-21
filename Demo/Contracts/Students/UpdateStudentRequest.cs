namespace Demo.Contracts.Students;

public sealed record UpdateStudentRequest(string FullName, string Email, DateOnly DateOfBirth);
