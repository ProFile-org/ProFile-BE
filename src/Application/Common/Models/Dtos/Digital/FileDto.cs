using Application.Common.Mappings;
using Domain.Entities.Digital;

namespace Application.Common.Models.Dtos.Digital;

public class FileDto : BaseDto, IMapFrom<FileEntity>
{
    public string FileType { get; set; } = null!;
    public string? FileExtension { get; set; }
}