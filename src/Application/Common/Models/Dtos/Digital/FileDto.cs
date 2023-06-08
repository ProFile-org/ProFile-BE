using Application.Common.Mappings;
using Domain.Entities.Digital;

namespace Application.Common.Models.Dtos.Digital;

public class FileDto : IMapFrom<FileEntity>
{
    public Guid Id { get; set; }
    public string FileType { get; set; } = null!;
}