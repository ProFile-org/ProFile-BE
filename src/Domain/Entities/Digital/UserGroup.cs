using Domain.Common;

namespace Domain.Entities.Digital;

public class UserGroup : BaseEntity
{
    public string Name { get; set; } = null!;
    public ICollection<User> Users { get; set; } = new List<User>();
}