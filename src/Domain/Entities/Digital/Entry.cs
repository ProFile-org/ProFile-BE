using Domain.Common;

namespace Domain.Entities.Digital;

public class Entry : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Path { get; set; } = null!;
    public FileEntity? File { get; set; }
}