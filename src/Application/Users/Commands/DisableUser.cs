using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Messages;
using Application.Users.Queries;
using AutoMapper;
using Domain.Entities.Logging;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Application.Users.Commands;

public class DisableUser
{
    public record Command : IRequest<UserDto>
    {
        public Guid PerformingUserId { get; init; }
        public Guid UserId { get; init; }
    }

    public class CommandHandler : IRequestHandler<Command, UserDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        public CommandHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<UserDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == request.UserId, cancellationToken);
            if (user is null)
            {
                throw new KeyNotFoundException("User does not exist.");
            }

            if (!user.IsActive)
            {
                throw new ConflictException("User has already been disabled.");
            }

            var performingUser = await _context.Users.FirstOrDefaultAsync(x => x.Id == request.PerformingUserId, cancellationToken);
            user.IsActive = false;
            user.LastModified = LocalDateTime.FromDateTime(DateTime.Now);
            user.LastModifiedBy = performingUser!.Id;
            var log = new UserLog()
            {
                User = performingUser,
                UserId = performingUser.Id,
                Object = user,
                Time = LocalDateTime.FromDateTime(DateTime.Now),
                Action = UserLogMessages.Disable,
            };
            var result = _context.Users.Update(user);
            await _context.UserLogs.AddAsync(log, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<UserDto>(result.Entity);
        }
    }
}