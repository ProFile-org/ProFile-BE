using Domain.Common;

namespace Domain.Entities.Physical;

public class Folder : BaseEntity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public Locker Locker { get; set; } = null!;
    public int Capacity { get; set; }
    public int NumberOfDocuments { get; set; }
    public bool IsAvailable { get; set; }
    
    // Navigation property
    public ICollection<Document> Documents { get; set; } = new List<Document>();
}