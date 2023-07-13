using Application.Common.Mappings;
using Application.Users.Queries;
using Domain.Entities.Physical;

namespace Application.Common.Models.Dtos.Physical;

public class DocumentItemDto : BaseDto, IMapFrom<Document>
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string DocumentType { get; set; } = null!;
    public DepartmentDto? Department { get; set; }
    public UserDto? Importer { get; set; }
    public FolderDto? Folder { get; set; }
}