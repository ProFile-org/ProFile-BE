using Application.Common.Mappings;
using AutoMapper;
using Domain.Entities.Physical;

namespace Application.Common.Models.Dtos.ImportDocument;

public class IssuedDocumentDto : BaseDto, IMapFrom<Document>
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string DocumentType { get; set; } = null!;
    public IssuerDto? Issuer { get; set; }
    public string Status { get; set; } = null!;
    public bool IsPrivate { get; set; }
        
    public void Mapping(Profile profile)
    {
        profile.CreateMap<Document, IssuedDocumentDto>()
            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Issuer,
                opt => opt.MapFrom(x => x.Importer));
    }
}