using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Users.Queries;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Commands;

public class DisableUser
{
    public record Command : IRequest<UserDto>
    {
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

            user.IsActive = false;

            var result = _context.Users.Update(user);
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<UserDto>(result.Entity);
        }
    }
}