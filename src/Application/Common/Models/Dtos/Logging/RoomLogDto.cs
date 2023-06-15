using Application.Common.Mappings;
using Application.Common.Models.Dtos.Physical;
using Application.Users.Queries;
using AutoMapper;
using Domain.Entities.Logging;

namespace Application.Common.Models.Dtos.Logging;

public class RoomLogDto : BaseDto, IMapFrom<RoomLog>
{
    public Guid UserId { get; set; } 
    public string Action { get; set; }
    public RoomDto? Object { get; set; }
    public DateTime Time { get; set; }
    public UserDto User { get; set; }

    public void Mapping(Profile profile)
    {
        
        profile.CreateMap<RoomLog, RoomLogDto>()
            .ForMember(dest => dest.Time,
                opt => opt.MapFrom(src => src.Time.ToDateTimeUnspecified()))
            .ForMember(dest => dest.Object,
                opt => opt.MapFrom(src => src.Object));
            
    }
}