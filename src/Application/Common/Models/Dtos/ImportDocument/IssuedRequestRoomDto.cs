using Application.Common.Mappings;
using AutoMapper;
using Domain.Entities.Physical;

namespace Application.Common.Models.Dtos.ImportDocument;

public class IssuedRequestRoomDto : BaseDto, IMapFrom<Room>
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public Guid? StaffId { get; set; }
    public DepartmentDto? Department { get; set; }
    
    public void Mapping(Profile profile)
    {
        profile.CreateMap<Room, IssuedRequestRoomDto>()
            .ForMember(dest => dest.StaffId,
                opt => opt.MapFrom(src => src.Staff!.Id));

    }
}