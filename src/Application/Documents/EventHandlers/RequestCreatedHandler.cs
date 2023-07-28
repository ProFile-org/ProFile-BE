using Application.Common.Interfaces;
using Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Documents.EventHandlers;

public class RequestCreatedHandler : INotificationHandler<RequestCreated>
{
    private readonly IApplicationDbContext _context;
    private readonly IMailService _mailService;

    public RequestCreatedHandler(IMailService mailService, IApplicationDbContext context)
    {
        _mailService = mailService;
        _context = context;
    }

    public async Task Handle(RequestCreated notification, CancellationToken cancellationToken)
    {
        var document = await _context.Documents
            .Include(x => x.Department)
            .FirstOrDefaultAsync(x => x.Id == notification.DocumentId, cancellationToken);
        
        var departmentId = document!.Department!.Id;
        
        var staff = await _context.Staffs
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Room!.DepartmentId == departmentId, cancellationToken);
        
        if (staff is not null)
        {
            _mailService.SendCreateRequestHtmlMail(notification.UserName, notification.RequestType, notification.Operation,
                notification.DocumentTitle, notification.Reason, notification.DocumentId, staff.User.Email);
        }
    }
}