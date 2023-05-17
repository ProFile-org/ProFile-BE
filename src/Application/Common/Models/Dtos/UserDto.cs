using Application.Common.Mappings;
using AutoMapper;
using Domain.Entities;

namespace Application.Users.Queries;

public class UserDto : IMapFrom<User>
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DepartmentDto Department { get; set; }
    public string Role { get; set; }
    public string Position { get; set; }
    public bool IsActive { get; set; }
    public bool IsActivated { get; set; }
}