using Application.Common.Mappings;
using Application.Common.Models.Dtos.Physical;
using Application.Users.Queries;
using AutoMapper;
using Domain.Entities.Logging;

namespace Application.Common.Models.Dtos.Logging;

public class DocumentLogDto : BaseDto, IMapFrom<DocumentLog>
{
    public Guid UserId { get; set; } 
    public string Action { get; set; }
    public Guid? ObjectId { get; set; }
    public DateTime Time { get; set; }
    public UserDto User { get; set; }

    public void Mapping(Profile profile)
    {

        profile.CreateMap<DocumentLog, DocumentLogDto>()
            .ForMember(dest => dest.Time,
                opt => opt.MapFrom(src => src.Time.ToDateTimeUnspecified()));

    }
}