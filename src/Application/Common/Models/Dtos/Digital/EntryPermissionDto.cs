using Application.Common.Mappings;
using Application.Common.Models.Operations;
using Application.Users.Queries;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Digital;

namespace Application.Common.Models.Dtos.Digital;

public class EntryPermissionDto : IMapFrom<EntryPermission>
{
    public Guid EmployeeId { get; set; }
    public Guid EntryId { get; set; }
    public bool CanView { get; set; }
    public bool CanEdit { get; set; }
    public bool IsSharedRoot { get; set; }

    public UserDto Employee { get; set; } = null!;

    public void Mapping(Profile profile)
    {
        profile.CreateMap<EntryPermission, EntryPermissionDto>()
            .ForMember(dest => dest.CanView,
                opt => opt.MapFrom(src => src.AllowedOperations.Split(',', StringSplitOptions.TrimEntries).Contains(EntryOperation.View.ToString())))
            .ForMember(dest => dest.CanEdit,
             opt => opt.MapFrom(src => src.AllowedOperations.Split(',', StringSplitOptions.TrimEntries).Contains(EntryOperation.Edit.ToString())));
    }
}