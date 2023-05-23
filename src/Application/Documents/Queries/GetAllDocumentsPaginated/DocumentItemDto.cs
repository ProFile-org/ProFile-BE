using Application.Common.Mappings;
using AutoMapper;
using Domain.Entities.Physical;

namespace Application.Documents.Queries.GetAllDocumentsPaginated;

public class DocumentItemDto : IMapFrom<Document>
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string DocumentType { get; set; } = null!;
    public Guid? DepartmentId { get; set; }
    public Guid? ImporterId { get; set; }
    public Guid? FolderId { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Document, DocumentItemDto>()
            .ForMember(dest => dest.DepartmentId,
                opt => opt.MapFrom(src => src.Department!.Id))
            .ForMember(dest => dest.ImporterId,
                opt => opt.MapFrom(src => src.Importer!.Id))
            .ForMember(dest => dest.FolderId,
                opt => opt.MapFrom(src => src.Folder!.Id));
    }
}