using Application.Common.Mappings;
using Application.Common.Models.Dtos.Physical;
using Domain.Entities.Physical;

namespace Application.Users.Queries.Physical;

public class StaffDto : IMapFrom<Staff>
{
    public UserDto User { get; set; }
    public RoomDto Room { get; set; }
}