using Application.Common.Interfaces;
using Application.Helpers;
using Domain.Entities;
using Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Application.Users.EventHandlers;

public class UserCreatedEventHandler : INotificationHandler<UserCreatedEvent>
{
    private readonly IMailService _mailService;
    private readonly IAuthDbContext _authDbContext;

    public UserCreatedEventHandler(IMailService mailService, IAuthDbContext authDbContext)
    {
        _mailService = mailService;
        _authDbContext = authDbContext;
    }

    public async Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken)
    {
        var expirationDate = LocalDateTime.FromDateTime(DateTime.Now.AddDays(1));

        var token = Guid.NewGuid().ToString();
        var resetPasswordToken = new ResetPasswordToken()
        {
            User = notification.User,
            TokenHash = SecurityUtil.Hash(token),
            ExpirationDate = expirationDate,
            IsInvalidated = false,
        };

        await _authDbContext.ResetPasswordTokens.AddAsync(resetPasswordToken, cancellationToken);
        await _authDbContext.SaveChangesAsync(cancellationToken);
        
        _mailService.SendResetPasswordHtmlMail(notification.User.Email, notification.Password, resetPasswordToken.TokenHash);
    }
}