using Domain.Common;
using Domain.Entities;

namespace Domain.Events;

public class StaffCreatedEvent : BaseEvent
{
    public StaffCreatedEvent(User staff, User currentUser)
    {
        Staff = staff;
        CurrentUser = currentUser;
    }
    
    public User Staff { get; }
    public User CurrentUser { get; }
}