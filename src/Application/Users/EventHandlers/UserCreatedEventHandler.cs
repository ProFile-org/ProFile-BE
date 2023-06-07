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
    private readonly IApplicationDbContext _applicationDbContext;
    private readonly IAuthDbContext _authDbContext;

    public UserCreatedEventHandler(IMailService mailService, IAuthDbContext authDbContext, IApplicationDbContext applicationDbContext)
    {
        _mailService = mailService;
        _applicationDbContext = applicationDbContext;
        _authDbContext = authDbContext;
    }

    public async Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken)
    {
        var user = await _applicationDbContext.Users.FirstAsync(
            x => x.Email.ToLower()
                .Equals(notification.Email.ToLower()), cancellationToken);

        var expirationDate = LocalDateTime.FromDateTime(DateTime.Now.AddDays(1));
        
        var resetPasswordToken = new ResetPasswordToken()
        {
            User = user,
            TokenHash = SecurityUtil.Hash(Guid.NewGuid().ToString()),
            ExpirationDate = expirationDate,
            IsInvalidated = false,
        };

        var tokenEntity = await _authDbContext.ResetPasswordTokens.AddAsync(resetPasswordToken, cancellationToken);
        await _authDbContext.SaveChangesAsync(cancellationToken);
        
        _mailService.SendResetPasswordHtmlMail(notification.Email, notification.Password, tokenEntity.Entity.TokenHash);
    }
}