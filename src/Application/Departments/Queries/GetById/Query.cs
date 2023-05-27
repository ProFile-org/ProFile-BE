using Application.Users.Queries;
using MediatR;

namespace Application.Departments.Queries.GetById;

public record Query : IRequest<DepartmentDto>
{
    public Guid DepartmentId { get; init; }
}