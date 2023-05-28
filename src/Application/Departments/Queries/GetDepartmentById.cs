using Application.Users.Queries;
using MediatR;

namespace Application.Departments.Queries;

public class GetDepartmentById
{
    public record Query : IRequest<DepartmentDto>
    {
        public Guid DepartmentId { get; init; }
    }
}