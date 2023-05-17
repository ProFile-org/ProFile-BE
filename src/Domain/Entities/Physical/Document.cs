using Domain.Common;

namespace Domain.Entities.Physical;

public class Document : BaseEntity
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string DocumentType { get; set; }
    public Department Department { get; set; }
    public User Importer { get; set; }
    public Folder Folder { get; set; }
}