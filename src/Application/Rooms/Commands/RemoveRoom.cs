using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Rooms.Commands;

public class RemoveRoom
{
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.RoomId)
                .NotEmpty().WithMessage("RoomId is required.");
        }   
    }
    
    public record Command : IRequest<RoomDto>
    {
        public Guid RoomId { get; init; }    
    }

    public class CommandHandler : IRequestHandler<Command, RoomDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CommandHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<RoomDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var room = await _context.Rooms
                .Include(x => x.Department)
                .FirstOrDefaultAsync(x => x.Id.Equals(request.RoomId), cancellationToken);
            
            if (room is null)
            {
                throw new KeyNotFoundException("Room does not exist.");
            }

            var containsDocuments = await _context.Documents
                .AnyAsync(x => x.Folder!.Locker.Room.Id == room.Id, cancellationToken);
            var containsFolders = await _context.Folders
                .AnyAsync(x => x.Locker.Room.Id == room.Id, cancellationToken);
            var containsLockers = await _context.Lockers
                .AnyAsync(x => x.Room.Id == room.Id, cancellationToken);
            if (containsDocuments || containsFolders || containsLockers)
            {
                throw new ConflictException("Room cannot be removed because it contains something.");
            }

            var result = _context.Rooms.Remove(room);
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<RoomDto>(result.Entity);
        }
    }
}