using Domain.Common;
using Domain.Entities;

namespace Domain.Events;

public class UserCreatedEvent : BaseEvent
{
    public UserCreatedEvent(string email, string password)
    {
        Email = email;
        Password = password;
    }

    public string Email { get; }
    public string Password { get; }
}