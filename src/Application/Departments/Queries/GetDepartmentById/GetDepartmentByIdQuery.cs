using Application.Users.Queries;
using MediatR;

namespace Application.Departments.Queries.GetDepartmentById;

public record GetDepartmentByIdQuery : IRequest<DepartmentDto>
{
    public Guid DepartmentId { get; init; }
}