using Domain.Common;
using NodaTime;

namespace Domain.Entities.Digital;

public class Entry : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Path { get; set; } = null!;
    public Guid? FileId { get; set; }
    public Guid UploaderId { get; set; }
    public LocalDateTime Created { get; set; }
    
    public virtual FileEntity? File { get; set; }
    public virtual User Uploader { get; set; } = null!;
}