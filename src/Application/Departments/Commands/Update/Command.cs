using Application.Users.Queries;
using MediatR;

namespace Application.Departments.Commands.Update;

public record Command : IRequest<DepartmentDto>
{
    public Guid DepartmentId { get; set; }
    public string Name { get; init; } = null!;
}