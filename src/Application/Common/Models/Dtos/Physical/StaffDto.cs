using Application.Common.Mappings;
using Application.Users.Queries;
using AutoMapper;
using Domain.Entities.Physical;

namespace Application.Common.Models.Dtos.Physical;

public class StaffDto : BaseDto, IMapFrom<Staff>
{
    public UserDto User { get; set; } = null!;
    public RoomDto? Room { get; set; }
    
    public void Mapping(Profile profile)
    {
        profile.CreateMap<Staff, StaffDto>()
            .ForMember(dest => dest.Id,
                opt => opt.MapFrom(src => src.User.Id));
    }
}