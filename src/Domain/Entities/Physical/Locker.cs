using Domain.Common;

namespace Domain.Entities.Physical;

public class Locker : BaseEntity
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public Room Room { get; set; }
    public int NumberOfFolders { get; set; }
    public int Capacity { get; set; }

    public bool IsAvailable { get; set; }
}