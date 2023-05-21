using Application.Common.Mappings;
<<<<<<< HEAD
using AutoMapper;
=======
>>>>>>> dev
using Domain.Entities.Physical;

namespace Application.Common.Models.Dtos.Physical;

public class FolderDto : IMapFrom<Folder>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public LockerDto Locker { get; set; }
    public int Capacity { get; set; }
    public int NumberOfDocuments { get; set; }
    public bool IsAvailable { get; set; }
}