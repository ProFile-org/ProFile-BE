using Application.Common.Mappings;
using AutoMapper;
using Domain.Entities.Physical;

namespace Application.Common.Models.Dtos.Physical;

public class RoomDto : IMapFrom<Room>
{
    public Guid Id { get; set; }
    public Guid StaffId { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public int NumberOfLockers { get; set; }
    public int Capacity { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Room, RoomDto>()
            .ForMember(x => x.StaffId,
                opt => opt.MapFrom(src => src.Staff!.Id));
    }
}