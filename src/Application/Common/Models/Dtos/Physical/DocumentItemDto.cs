using Application.Common.Mappings;
using Application.Users.Queries;
using Domain.Entities.Physical;

namespace Application.Common.Models.Dtos.Physical;

[Obsolete]
public class DocumentItemDto : IMapFrom<Document>
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string DocumentType { get; set; } = null!;
    public DepartmentDto? Department { get; set; }
    public UserDto? Importer { get; set; }
    public FolderDto? Folder { get; set; }
}