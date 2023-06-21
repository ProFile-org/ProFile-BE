using Application.Common.Mappings;
using Domain.Entities.Digital;

namespace Application.Common.Models.Dtos.Digital;

public class UserGroupDto : BaseDto, IMapFrom<UserGroup>
{
    public string Name { get; set; } = null!;
}