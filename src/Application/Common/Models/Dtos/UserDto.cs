using Application.Common.Mappings;
using Application.Common.Models.Dtos;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Digital;

namespace Application.Users.Queries;

public class UserDto : BaseDto, IMapFrom<User>
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DepartmentDto Department { get; set; }
    public string Role { get; set; }
    public string Position { get; set; }
    public bool IsActive { get; set; }
    public bool IsActivated { get; set; }
    public DateTime Created { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? LastModified { get; set; }
    public Guid? LastModifiedBy { get; set; }

    public IEnumerable<UserGroup> UserGroups { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<User, UserDto>()
            .ForMember(dest => dest.Created,
                opt => opt.MapFrom(src => src.Created.ToDateTimeUnspecified()))
            .ForMember(dest => dest.LastModified,
                opt => opt.MapFrom(src => src.LastModified.Value.ToDateTimeUnspecified()));
    }
}