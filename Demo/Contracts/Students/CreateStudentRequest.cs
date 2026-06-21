namespace Demo.Contracts.Students;

public sealed record CreateStudentRequest(string StudentCode, string FullName, string Email, DateOnly DateOfBirth);
