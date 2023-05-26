using Application.Users.Queries;
using MediatR;

namespace Application.Departments.Commands.UpdateDepartment;

public record UpdateDepartmentCommand : IRequest<DepartmentDto>
{
    public Guid DepartmentId { get; set; }
    public string Name { get; init; } = null!;
}