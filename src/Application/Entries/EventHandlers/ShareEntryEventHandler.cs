using Application.Common.Interfaces;
using Domain.Events;
using MediatR;

namespace Application.Entries.EventHandlers;

public class ShareEntryEventHandler : INotificationHandler<ShareEntryEvent>
{
    private readonly IMailService _mailService;

    public ShareEntryEventHandler(IMailService mailService)
    {
        _mailService = mailService;
    }

    public async Task Handle(ShareEntryEvent notification, CancellationToken cancellationToken)
    {
        _mailService.SendShareEntryHtmlMail(notification.IsDirectory, notification.EntryName, notification.SharerName,
            notification.Operation, notification.OwnerName, notification.SharedUserEmail, notification.Path);
    }
}