using Domain.Common;
using Domain.Entities;

namespace Domain.Events;

public class UserCreatedEvent : BaseEvent
{
    public UserCreatedEvent(User user, string password)
    {
        User = user;
        Password = password;
    }

    public User User { get; }
    public string Password { get; }
}