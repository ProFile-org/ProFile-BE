using Application.Common.Interfaces;
using Domain.Entities.Physical;
using Domain.Events;
using MediatR;

namespace Application.Staffs.EventHandlers;

public class StaffCreatedEventHandler : INotificationHandler<StaffCreatedEvent>
{
    private readonly IApplicationDbContext _context;

    public StaffCreatedEventHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(StaffCreatedEvent notification, CancellationToken cancellationToken)
    {
        var staff = new Staff()
        {
            Id = notification.Staff.Id,
            User = notification.Staff,
        };
        
        await _context.Staffs.AddAsync(staff, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}