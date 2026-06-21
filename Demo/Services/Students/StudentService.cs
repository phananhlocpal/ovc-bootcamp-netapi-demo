using System.Collections.Concurrent;
using Demo.Contracts.Students;
using Demo.Exceptions;
using Demo.Models;

namespace Demo.Services.Students;

public sealed class StudentService : IStudentService
{
    private readonly ConcurrentDictionary<Guid, Student> _students = new();

    public StudentService()
    {
        var now = DateTime.UtcNow;
        var seedStudents = new[]
        {
            new Student
            {
                Id = Guid.NewGuid(),
                StudentCode = "STU001",
                FullName = "Nguyen Van A",
                Email = "nguyenvana@example.com",
                DateOfBirth = new DateOnly(2002, 5, 20),
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            },
            new Student
            {
                Id = Guid.NewGuid(),
                StudentCode = "STU002",
                FullName = "Tran Thi B",
                Email = "tranthib@example.com",
                DateOfBirth = new DateOnly(2001, 11, 14),
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            }
        };

        foreach (var student in seedStudents)
        {
            _students[student.Id] = student;
        }
    }

    public Task<IReadOnlyCollection<StudentResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        IReadOnlyCollection<StudentResponse> result = _students.Values
            .OrderBy(student => student.StudentCode)
            .Select(MapToResponse)
            .ToArray();

        return Task.FromResult(result);
    }

    public Task<StudentResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var student = GetRequiredStudent(id);
        return Task.FromResult(MapToResponse(student));
    }

    public Task<StudentResponse> CreateAsync(CreateStudentRequest request, CancellationToken cancellationToken)
    {
        EnsureUniqueStudentCode(request.StudentCode);

        var now = DateTime.UtcNow;
        var student = new Student
        {
            Id = Guid.NewGuid(),
            StudentCode = request.StudentCode.Trim(),
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            DateOfBirth = request.DateOfBirth,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        _students[student.Id] = student;
        return Task.FromResult(MapToResponse(student));
    }

    public Task<StudentResponse> UpdateAsync(Guid id, UpdateStudentRequest request, CancellationToken cancellationToken)
    {
        var student = GetRequiredStudent(id);
        student.FullName = request.FullName.Trim();
        student.Email = request.Email.Trim().ToLowerInvariant();
        student.DateOfBirth = request.DateOfBirth;
        student.UpdatedAtUtc = DateTime.UtcNow;

        return Task.FromResult(MapToResponse(student));
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!_students.TryRemove(id, out _))
        {
            throw new NotFoundException($"Student with id '{id}' was not found.");
        }

        return Task.CompletedTask;
    }

    private Student GetRequiredStudent(Guid id)
    {
        if (_students.TryGetValue(id, out var student))
        {
            return student;
        }

        throw new NotFoundException($"Student with id '{id}' was not found.");
    }

    private void EnsureUniqueStudentCode(string studentCode)
    {
        var normalizedCode = studentCode.Trim();
        var exists = _students.Values.Any(student =>
            string.Equals(student.StudentCode, normalizedCode, StringComparison.OrdinalIgnoreCase));

        if (exists)
        {
            throw new AppException($"Student code '{normalizedCode}' already exists.", StatusCodes.Status409Conflict);
        }
    }

    private static StudentResponse MapToResponse(Student student)
    {
        return new StudentResponse(
            student.Id,
            student.StudentCode,
            student.FullName,
            student.Email,
            student.DateOfBirth,
            student.CreatedAtUtc,
            student.UpdatedAtUtc);
    }
}
