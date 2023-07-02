using Application.Common.Mappings;
using Application.Common.Models.Operations;
using AutoMapper;
using Domain.Entities.Digital;

namespace Application.Common.Models.Dtos.Digital;

public class EntryPermissionDto : IMapFrom<EntryPermission>
{
    public Guid EmployeeId { get; set; }
    public Guid EntryId { get; set; }
    public bool CanView { get; set; }
    public bool CanUpload { get; set; }
    public bool CanDownload { get; set; }
    public bool CanChangePermission { get; set; }
    
    public void Mapping(Profile profile)
    {
        profile.CreateMap<EntryPermission, EntryPermissionDto>()
            .ForMember(dest => dest.CanView,
                opt => opt.MapFrom(src => src.AllowedOperations.Contains(EntryOperation.View.ToString())))
            .ForMember(dest => dest.CanUpload,
                opt => opt.MapFrom(src => src.AllowedOperations.Contains(EntryOperation.Upload.ToString())))
            .ForMember(dest => dest.CanDownload,
                opt => opt.MapFrom(src => src.AllowedOperations.Contains(EntryOperation.Download.ToString())))
            .ForMember(dest => dest.CanChangePermission,
                opt => opt.MapFrom(src => src.AllowedOperations.Contains(EntryOperation.ChangePermission.ToString())));
    }
}