using Application.Common.Mappings;
using AutoMapper;
using Domain.Entities.Physical;

namespace Application.Common.Models.Dtos.Physical;

public class EmptyFolderDto : BaseDto, IMapFrom<Folder>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Capacity { get; set; }
    public int Slot { get; set; }
    
    public void Mapping(Profile profile)
    {
        profile.CreateMap<Folder, EmptyFolderDto>()
            .ForMember(dest => dest.Slot,
                opt => opt.MapFrom(src => src.Capacity - src.NumberOfDocuments));
    }
}