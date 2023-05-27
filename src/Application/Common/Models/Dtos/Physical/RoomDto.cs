using Application.Common.Mappings;
using AutoMapper;
using Domain.Entities.Physical;

namespace Application.Common.Models.Dtos.Physical;

public class RoomDto : IMapFrom<Room>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public StaffDto Staff { get; set; }
    public int Capacity { get; set; }
    public int NumberOfLockers { get; set; }
    public bool IsAvailable { get; set; }
}