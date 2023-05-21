using Application.Common.Mappings;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using Domain.Entities.Physical;

namespace Application.Rooms.Queries.GetEmptyContainersPaginated;

public class EmptyLockerDto : IMapFrom<Locker>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int NumberOfFreeFolders { get; set; }
    public ICollection<EmptyFolderDto> Folders { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Locker, EmptyLockerDto>()
            .ForMember(dest => dest.NumberOfFreeFolders,
                opt => opt.MapFrom(src => src.Folders.Count(x => x.NumberOfDocuments < x.Capacity)));
    }
}