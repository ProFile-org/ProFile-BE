using Domain.Common;

namespace Domain.Entities.Physical;

public class Staff : BaseEntity
{
    public User User { get; set; } = null!;
    public Room? Room { get; set; }
}