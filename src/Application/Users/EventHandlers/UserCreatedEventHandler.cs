using Application.Common.Interfaces;
using Domain.Events;
using MediatR;

namespace Application.Users.EventHandlers;

public class UserCreatedEventHandler : INotificationHandler<UserCreatedEvent>
{
    private readonly IMailService _mailService;

    public UserCreatedEventHandler(IMailService mailService)
    {
        _mailService = mailService;
    }

    public async Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken)
    {
        _mailService.SendResetPasswordHtmlMail(notification.Email, notification.Password, "");
    }
}