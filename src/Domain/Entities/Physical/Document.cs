using Domain.Common;
using Domain.Entities.Digital;
using Domain.Statuses;

namespace Domain.Entities.Physical;

public class Document : BaseEntity
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string DocumentType { get; set; } = null!;
    public Department? Department { get; set; }
    public User? Importer { get; set; }
    public Folder? Folder { get; set; }
    public DocumentStatus Status { get; set; }
    public Guid? EntryId { get; set; }
    
    public virtual Entry? Entry { get; set; }
}