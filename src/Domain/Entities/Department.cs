using Domain.Common;
using Domain.Entities.Physical;

namespace Domain.Entities;

public class Department : BaseEntity
{
    public string Name { get; set; } = null!;
    public Guid? RoomId { get; set; }
    public Room? Room { get; set; }
}