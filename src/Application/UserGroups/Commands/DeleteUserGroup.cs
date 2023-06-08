using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Digital;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.UserGroups.Commands;

public class DeleteUserGroup
{
    public record Command : IRequest<UserGroupDto>
    {
        public Guid UserGroupId { get; init; }
    }
    
    public class CommandHandler : IRequestHandler<Command, UserGroupDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CommandHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        
        public async Task<UserGroupDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var userGroup = await _context.UserGroups.FirstOrDefaultAsync(x => x.Id == request.UserGroupId, cancellationToken);

            if (userGroup is null)
            {
                throw new KeyNotFoundException("User group does not exist.");
            }

            var result = _context.UserGroups.Remove(userGroup);
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<UserGroupDto>(result.Entity);
        }
    }
}
