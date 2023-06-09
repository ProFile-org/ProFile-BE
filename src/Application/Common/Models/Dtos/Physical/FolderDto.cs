using Application.Common.Mappings;
using Domain.Entities.Physical;

namespace Application.Common.Models.Dtos.Physical;

public class FolderDto : BaseDto, IMapFrom<Folder>
{
    public string Name { get; set; }
    public string Description { get; set; }
    public LockerDto Locker { get; set; }
    public int Capacity { get; set; }
    public int NumberOfDocuments { get; set; }
    public bool IsAvailable { get; set; }
}