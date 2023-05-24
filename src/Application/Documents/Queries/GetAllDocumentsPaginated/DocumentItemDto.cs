using Application.Common.Mappings;
using Application.Common.Models.Dtos.Physical;
using Application.Users.Queries;
using AutoMapper;
using Domain.Entities.Physical;

namespace Application.Documents.Queries.GetAllDocumentsPaginated;

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