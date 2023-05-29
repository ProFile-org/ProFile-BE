using Application.Common.Mappings;
using Application.Users.Queries;
using Domain.Entities.Physical;

namespace Application.Common.Models.Dtos.Physical;

public class StaffDto : IMapFrom<Staff>
{
    public UserDto User { get; set; } = null!;
    public RoomDto? Room { get; set; }
}