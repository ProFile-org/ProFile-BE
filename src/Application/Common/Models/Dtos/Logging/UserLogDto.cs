using Application.Common.Mappings;
using Application.Users.Queries;
using AutoMapper;
using Domain.Entities.Logging;

namespace Application.Common.Models.Dtos.Logging;

public class UserLogDto : IMapFrom<UserLog>
{
    public Guid Id { get; set; }
    public string Action { get; set; } = null!;
    public UserDto? Object { get; set; }
    public DateTime Time { get; set; }
    public UserDto User { get; set; } = null!;

    public void Mapping(Profile profile)
    {
        profile.CreateMap<UserLog, UserLogDto>()
            .ForMember( dest => dest.Time, 
                opt => opt.MapFrom( src => src.Time.ToDateTimeUnspecified()))
            .ForMember(dest => dest.Object, 
                opt => opt.MapFrom( src => src.Object));
        
    }
}