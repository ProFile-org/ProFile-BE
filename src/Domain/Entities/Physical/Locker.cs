using Domain.Common;

namespace Domain.Entities.Physical;

public class Locker : BaseAuditableEntity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public Room Room { get; set; } = null!;
    public int Capacity { get; set; }
    public int NumberOfFolders { get; set; }
    public bool IsAvailable { get; set; }

    // Navigation property
    public ICollection<Folder> Folders { get; set; } = new List<Folder>();
}