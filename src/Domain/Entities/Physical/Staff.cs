using Domain.Common;

namespace Domain.Entities.Physical;

public class Staff : BaseEntity
{
    public Guid RoomId { get; set; }
    public User User { get; set; }
    public Room Room { get; set; }
}