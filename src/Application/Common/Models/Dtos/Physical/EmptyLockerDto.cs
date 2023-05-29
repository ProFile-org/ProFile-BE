using Application.Common.Mappings;
using AutoMapper;
using Domain.Entities.Physical;

namespace Application.Common.Models.Dtos.Physical;

public class EmptyLockerDto : IMapFrom<Locker>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Capacity { get; set; }
    public int NumberOfFolders { get; set; }
    public int NumberOfFreeFolders { get; set; }
    public IEnumerable<EmptyFolderDto> Folders { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Locker, EmptyLockerDto>()
            .ForMember(dest => dest.NumberOfFreeFolders,
                opt => opt.MapFrom(src => src.Folders.Count(x => x.NumberOfDocuments < x.Capacity)));
    }
}