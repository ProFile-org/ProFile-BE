using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Staffs.Commands;

public class RemoveStaffFromRoom
{
    public record Command : IRequest<StaffDto>
    {
        public Guid StaffId { get; init; }
    }
    
    public class CommandHandler : IRequestHandler<Command, StaffDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CommandHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<StaffDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var staff = await _context.Staffs
                .Include(x => x.User)
                .Include(x => x.Room)
                .FirstOrDefaultAsync(x => x.User.Id.Equals(request.StaffId), cancellationToken: cancellationToken);

            if (staff is null)
            {
                throw new KeyNotFoundException("Staff does not exist.");
            }

            if (staff.Room is null)
            {
                throw new ConflictException("Staff is not assigned to a room.");
            }

            staff.Room.Staff = null;
            _context.Rooms.Update(staff.Room!);
            staff.Room = null;
            var result = _context.Staffs.Update(staff);
            await _context.SaveChangesAsync(cancellationToken);

            return _mapper.Map<StaffDto>(result.Entity);
        }
    }
}