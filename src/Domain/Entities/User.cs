using Domain.Common;
using Domain.Entities.Physical;

namespace Domain.Entities;

public class User : BaseAuditableEntity
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public Department Department { get; set; }
    public string Role { get; set; }
    public string Position { get; set; }
    public bool IsActive { get; set; }
    public bool IsActivated { get; set; }
    
    // Navigation properties
    public Staff Staff { get; set; }
}