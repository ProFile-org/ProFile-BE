using Application.Common.Mappings;
using Application.Users.Queries;
using AutoMapper;
using Domain.Entities.Digital;

namespace Application.Common.Models.Dtos.Digital;

public class EntryDto : BaseDto, IMapFrom<Entry>
{
    public string Name { get; set; } = null!;
    public string Path { get; set; } = null!;
    public FileDto? File { get; set; }
    public DateTime Created { get; set; }
    public UserDto Uploader { get; set; } = null!;

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Entry, EntryDto>()
            .ForMember(dest => dest.Created, 
                opt => opt.MapFrom(src => src.Created));
    }
}