using Application.Common.Mappings;
using Domain.Entities;

namespace Application.Common.Models.Dtos.ImportDocument;

public class IssuerDto : IMapFrom<User>
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Role { get; set; }
    public string Position { get; set; }
    public bool IsActive { get; set; }
    public bool IsActivated { get; set; }
}