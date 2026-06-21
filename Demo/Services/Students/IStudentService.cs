using Demo.Contracts.Students;

namespace Demo.Services.Students;

public interface IStudentService
{
    Task<IReadOnlyCollection<StudentResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<StudentResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<StudentResponse> CreateAsync(CreateStudentRequest request, CancellationToken cancellationToken);
    Task<StudentResponse> UpdateAsync(Guid id, UpdateStudentRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
