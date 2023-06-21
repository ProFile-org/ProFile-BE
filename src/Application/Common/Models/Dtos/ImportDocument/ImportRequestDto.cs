using Application.Common.Mappings;
using AutoMapper;
using Domain.Entities.Physical;

namespace Application.Common.Models.Dtos.ImportDocument;

public class ImportRequestDto : BaseDto, IMapFrom<ImportRequest>
{
    public IssuedRequestRoomDto Room { get; set; } = null!;
    public IssuedDocumentDto Document { get; set; } = null!;
    public string ImportReason { get; set; } = null!;
    public string StaffReason { get; set; } = null!;
    public string Status { get; set; } = null!;

    public void Mapping(Profile profile)
    {
        profile.CreateMap<ImportRequest, ImportRequestDto>()
            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => src.Status.ToString()));
        
    }
}