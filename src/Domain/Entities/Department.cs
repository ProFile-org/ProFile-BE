using Domain.Common;
using Domain.Entities.Physical;

namespace Domain.Entities;

public class Department : BaseEntity
{
    public string Name { get; set; } = null!;
    
    public ICollection<Room> Rooms { get; set; } = new List<Room>();
}