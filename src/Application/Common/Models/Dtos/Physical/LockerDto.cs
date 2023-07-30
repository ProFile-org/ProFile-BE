using Application.Common.Mappings;
using Domain.Entities.Physical;

namespace Application.Common.Models.Dtos.Physical;

public class LockerDto : BaseDto, IMapFrom<Locker>
{
    public string Name { get; set; }
    public string Description { get; set; }
    public RoomDto Room { get; set; }
    public int NumberOfFolders { get; set; }
    public int Capacity { get; set; }
    public bool IsAvailable { get; set; }
}