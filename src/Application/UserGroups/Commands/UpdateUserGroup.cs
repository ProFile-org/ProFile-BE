using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Digital;
using AutoMapper;
using Domain.Entities.Digital;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.UserGroups.Commands;

public class UpdateUserGroup
{
    public record Command : IRequest<UserGroupDto>
    {
        public Guid UserGroupId { get; init; }
        public string Name { get; init; } = null!;
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
            var userGroup = await _context.UserGroups
                .FirstOrDefaultAsync(x => x.Id == request.UserGroupId, cancellationToken);

            if (userGroup is null)
            {
                throw new KeyNotFoundException("User group does not exist.");
            }

            var existedUserGroup = await _context.UserGroups
                .FirstOrDefaultAsync(x => x.Name.Equals(request.Name) 
                                          && x.Id != userGroup.Id, cancellationToken);

            if (existedUserGroup is not null)
            {
                throw new ConflictException("New user group name already exists.");
            }

            var updatedUserGroup = new UserGroup()
            {
                Id = userGroup.Id,
                Name = request.Name,
                Users = userGroup.Users,
            };
            
            _context.UserGroups.Entry(userGroup).State = EntityState.Detached;
            _context.UserGroups.Entry(updatedUserGroup).State = EntityState.Modified;
            
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<UserGroupDto>(updatedUserGroup);
        }
    }
}