using Application.Common.Mappings;
using Application.Common.Models.Operations;
using Application.Users.Queries;
using AutoMapper;
using Domain.Entities.Physical;

namespace Application.Common.Models.Dtos.Physical;

public class PermissionDto : IMapFrom<Permission>
{
    public bool CanRead { get; set; }
    public bool CanBorrow { get; set; }
    public Guid EmployeeId { get; set; }
    public Guid DocumentId { get; set; }
    
    public UserDto Employee { get; set; } = null!;

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Permission, PermissionDto>()
            .ForMember(dest => dest.CanRead,
                opt => opt.MapFrom(src => src.AllowedOperations.Contains(DocumentOperation.Read.ToString())))
            .ForMember(dest => dest.CanBorrow,
                opt => opt.MapFrom(src => src.AllowedOperations.Contains(DocumentOperation.Borrow.ToString())));
    }
}