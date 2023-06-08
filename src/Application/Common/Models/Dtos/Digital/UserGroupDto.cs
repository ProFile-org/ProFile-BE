using Application.Common.Mappings;
using Domain.Entities.Digital;

namespace Application.Common.Models.Dtos.Digital;

public class UserGroupDto : IMapFrom<UserGroup>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
}