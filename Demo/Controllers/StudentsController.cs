using Demo.Authorization;
using Demo.Contracts.Students;
using Demo.Services.Students;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class StudentsController(IStudentService studentService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = AppPolicies.CanReadStudents)]
    [ProducesResponseType(typeof(IReadOnlyCollection<StudentResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<StudentResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var students = await studentService.GetAllAsync(cancellationToken);
        return Ok(students);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = AppPolicies.CanReadStudents)]
    [ProducesResponseType(typeof(StudentResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<StudentResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var student = await studentService.GetByIdAsync(id, cancellationToken);
        return Ok(student);
    }

    [HttpPost]
    [Authorize(Policy = AppPolicies.CanManageStudents)]
    [ProducesResponseType(typeof(StudentResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<StudentResponse>> Create([FromBody] CreateStudentRequest request, CancellationToken cancellationToken)
    {
        var student = await studentService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = student.Id }, student);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = AppPolicies.CanManageStudents)]
    [ProducesResponseType(typeof(StudentResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<StudentResponse>> Update(Guid id, [FromBody] UpdateStudentRequest request, CancellationToken cancellationToken)
    {
        var student = await studentService.UpdateAsync(id, request, cancellationToken);
        return Ok(student);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = AppRoles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await studentService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
