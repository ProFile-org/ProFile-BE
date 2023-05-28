using Domain.Common;

namespace Domain.Entities.Physical;

public class Room : BaseEntity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public Staff? Staff { get; set; }
    public Guid DepartmentId { get; set; }
    public int Capacity { get; set; }
    public int NumberOfLockers { get; set; }
    public bool IsAvailable { get; set; }

    // Navigation property
    public Department Department { get; set; } = null!;
    public ICollection<Locker> Lockers { get; set; } = new List<Locker>();
}