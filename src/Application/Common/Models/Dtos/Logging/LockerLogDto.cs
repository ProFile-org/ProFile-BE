using Application.Common.Mappings;
using Application.Common.Models.Dtos.Physical;
using Application.Users.Queries;
using AutoMapper;
using Domain.Entities.Logging;

namespace Application.Common.Models.Dtos.Logging;

public class LockerLogDto : BaseDto, IMapFrom<LockerLog>
{
    public string Action { get; set; } = null!;
    public Guid? ObjectId { get; set; }
    public DateTime Time { get; set; }
    public UserDto User { get; set; } = null!;
    
    public void Mapping(Profile profile)
    {
        profile.CreateMap<LockerLog, LockerLogDto>()
            .ForMember( dest => dest.Time, 
                opt => opt.MapFrom( src => src.Time.ToDateTimeUnspecified()));
    }
}