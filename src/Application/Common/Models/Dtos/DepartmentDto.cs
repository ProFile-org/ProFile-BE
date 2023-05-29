using Application.Common.Mappings;
using Application.Common.Models.Dtos.Physical;
using Domain.Entities;

namespace Application.Common.Models.Dtos;

public class DepartmentDto : IMapFrom<Department>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public RoomDto? Room { get; set; }
}