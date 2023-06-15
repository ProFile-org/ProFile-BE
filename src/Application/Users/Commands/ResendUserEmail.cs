using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Helpers;
using Application.Users.Queries;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Application.Users.Commands;

public class ResendUserEmail
{
    public record Command : IRequest<UserDto> 
    {
        public Guid UserId { get; init; }
    }
    
    public class CommandHandler : IRequestHandler<Command, UserDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMailService _mailService;
        private readonly IAuthDbContext _authContext;
        private readonly IMapper _mapper;
        private readonly ISecurityService _securityService; 
        
        public CommandHandler(IApplicationDbContext context, IMailService mailService, IAuthDbContext authContext, IMapper mapper, ISecurityService securityService)
        {
            _context = context;
            _mailService = mailService;
            _authContext = authContext;
            _mapper = mapper;
            _securityService = securityService;
        }

        public async Task<UserDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var token = Guid.NewGuid().ToString();
            var password = StringUtil.RandomPassword();
            var salt = StringUtil.RandomSalt();
            var expirationDate = LocalDateTime.FromDateTime(DateTime.Now.AddDays(1));

            var user = await _context.Users
                .Include(x => x.Department)
                .FirstOrDefaultAsync(x => x.Id.Equals(request.UserId),cancellationToken);

            if (user is null)
            {
                throw new KeyNotFoundException("User does not exist");
            }
            
            var resetPasswordToken = new ResetPasswordToken()
            {
                User = user,
                TokenHash = SecurityUtil.Hash(token),
                ExpirationDate = expirationDate,
                IsInvalidated = false,
            };

            var pastResetPasswordToken = await _authContext.ResetPasswordTokens
                    .FirstOrDefaultAsync(x => x.User.Id.Equals(user.Id), cancellationToken);

            if (pastResetPasswordToken is null)
            {
                throw new KeyNotFoundException("Token does not exist.");
            }

            _authContext.ResetPasswordTokens.Remove(pastResetPasswordToken);
            await _authContext.ResetPasswordTokens.AddAsync(resetPasswordToken, cancellationToken);
            user.PasswordSalt = salt ;
            user.PasswordHash =_securityService.Hash(password, salt);
            await _authContext.SaveChangesAsync(cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            _mailService.SendResetPasswordHtmlMail(user.Email, password, token);
            return _mapper.Map<UserDto>(user);
        }
    }
}