using Application.Common.Mappings;
using AutoMapper;
using Domain.Entities.Physical;

namespace Application.Common.Models.Dtos.Physical;

public class FolderDto : IMapFrom<Folder>
{
    public Guid Id { get; set; }
    public Guid LockerId { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public int Capacity { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Folder, FolderDto>();
    }
}