using Application.Common.Mappings;
using Application.Users.Queries;
using AutoMapper;
using Domain.Entities.Digital;

namespace Application.Common.Models.Dtos.Digital;

public class EntryDto : BaseDto, IMapFrom<Entry>
{
    public string Name { get; set; } = null!;
    public string Path { get; set; } = null!;
    public Guid? FileId { get; set; }
    public string? FileType { get; set; }
    public string? FileExtension { get; set; }
    public bool IsDirectory { get; set; }
    public long? SizeInBytes { get; set; }
    public UserDto Owner { get; set; } = null!;
    public DateTime Created { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? LastModified { get; set; }
    public Guid? LastModifiedBy { get; set; }
    public UserDto Uploader { get; set; } = null!;

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Entry, EntryDto>()
            .ForMember(dest => dest.Created,
                opt => opt.MapFrom(src => src.Created.ToDateTimeUnspecified()))
            .ForMember(dest => dest.LastModified,
                opt => opt.MapFrom(src => src.LastModified.Value.ToDateTimeUnspecified()))
            .ForMember(dest => dest.FileType,
                opt => opt.MapFrom(src => src.File.FileType))
            .ForMember(dest => dest.FileExtension,
                opt => opt.MapFrom(src => src.File.FileExtension));
    }
}