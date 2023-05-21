using Application.Common.Mappings;
using Application.Users.Queries;
using Domain.Entities.Physical;

namespace Application.Common.Models.Dtos.Physical;

public class DocumentDto : IMapFrom<Document>
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string DocumentType { get; set; }
    public DepartmentDto Department { get; set; }
    public UserDto Importer { get; set; }
    public FolderDto Folder { get; set; }
}