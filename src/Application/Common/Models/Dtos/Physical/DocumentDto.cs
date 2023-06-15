using Application.Common.Mappings;
using Application.Common.Models.Dtos.Digital;
using Application.Users.Queries;
using AutoMapper;
using Domain.Entities.Physical;

namespace Application.Common.Models.Dtos.Physical;

public class DocumentDto : BaseDto, IMapFrom<Document>
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string DocumentType { get; set; } = null!;
    public DepartmentDto? Department { get; set; }
    public UserDto? Importer { get; set; }
    public FolderDto? Folder { get; set; }
    public string Status { get; set; } = null!;
    public EntryDto? Entry { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Document, DocumentDto>()
            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => src.Status.ToString()));
    }
}