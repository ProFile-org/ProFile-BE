using Domain.Common;
using Domain.Entities.Digital;
using Domain.Entities.Physical;

namespace Domain.Entities;

public class User : BaseAuditableEntity
{
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public Department? Department { get; set; }
    public string Role { get; set; } = null!;
    public string? Position { get; set; }
    public bool IsActive { get; set; }
    public bool IsActivated { get; set; }
    
    public ICollection<UserGroup> UserGroups { get; set; } = new List<UserGroup>();
}