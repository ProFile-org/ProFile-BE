using Application.Common.Mappings;
using Domain.Entities;

namespace Application.Users.Queries;

public class UserDTO : IMapFrom<User>
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
}