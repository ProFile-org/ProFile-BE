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

    public class RemoveRoomCommandHandler : IRequestHandler<Command, RoomDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public RemoveRoomCommandHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<RoomDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var room = await _context.Rooms
                .FirstOrDefaultAsync(x => x.Id.Equals(request.RoomId), cancellationToken: cancellationToken);

            if (room is null)
            {
                throw new KeyNotFoundException("Room does not exist.");
            }

            var canNotRemove = await _context.Documents
                                   .CountAsync(x => x.Folder!.Locker.Room.Id.Equals(request.RoomId), cancellationToken: cancellationToken)
                               > 0;
        
            if (canNotRemove)
            {
                throw new InvalidOperationException("Room cannot be removed because it contains documents.");
            }

            room.IsAvailable = false;
            var result = _context.Rooms.Remove(room);
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<RoomDto>(result.Entity);
        }
    }
}