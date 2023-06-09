using Domain.Common;

namespace Domain.Entities.Physical;

public class Locker : BaseEntity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public Room Room { get; set; } = null!;
    public User? Owner { get; set; }
    public int Capacity { get; set; }
    public int NumberOfFolders { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsPrivate { get; set; }

    // Navigation property
    public ICollection<Folder> Folders { get; set; } = new List<Folder>();
}