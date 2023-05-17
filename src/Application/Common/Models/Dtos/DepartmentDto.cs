using Application.Common.Mappings;
using Domain.Entities;

namespace Application.Users.Queries;

public class DepartmentDto : IMapFrom<Department>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}