using Application.Common.Mappings;
using Application.Users.Queries;
using AutoMapper;
using Domain.Entities.Physical;

namespace Application.Common.Models.Dtos.Physical;

public class RoomDto : IMapFrom<Room>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public Guid? StaffId { get; set; }
    public DepartmentDto? Department { get; set; }
    public int Capacity { get; set; }
    public int NumberOfLockers { get; set; }
    public bool IsAvailable { get; set; }
    
    public void Mapping(Profile profile)
    {
        profile.CreateMap<Room, RoomDto>()
            .ForMember(dest => dest.StaffId,
                opt => opt.MapFrom(src => src.Staff!.Id));
    }
}