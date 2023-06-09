using System.ComponentModel.DataAnnotations.Schema;
using Domain.Common;

namespace Domain.Entities.Digital;

public class Entry : BaseAuditableEntity
{
    public string Name { get; set; } = null!;
    public string Path { get; set; } = null!;
    public string? OldPath { get; set; }
    public Guid? FileId { get; set; }
    public Guid OwnerId { get; set; }
    public long? SizeInBytes { get; set; }
    [NotMapped] public bool IsDirectory => FileId is null;
    [NotMapped] public bool IsInBin => OldPath is not null;

    public virtual FileEntity? File { get; set; }
    public virtual User Uploader { get; set; } = null!;
    public virtual User Owner { get; set; } = null!;
}