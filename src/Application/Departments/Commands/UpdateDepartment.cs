using Application.Common.Models.Dtos;
using Application.Users.Queries;
using MediatR;

namespace Application.Departments.Commands;

public class UpdateDepartment
{
    public record Command : IRequest<DepartmentDto>
    {
        public Guid DepartmentId { get; set; }
        public string Name { get; init; } = null!;
    }
}