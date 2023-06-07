using Domain.Common;

namespace Domain.Entities.Digital;

public class FileEntity : BaseEntity
{
    public string FileType { get; set; } = null!;
    public byte[] FileData { get; set; } = null!;
}