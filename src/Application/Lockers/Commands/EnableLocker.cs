using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Lockers.Commands;

public class EnableLocker
{
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;
        
            RuleFor(x => x.LockerId)
                .NotEmpty().WithMessage("LockerId is required.");
        }
    }
    
    public record Command : IRequest<LockerDto>
    {
        public Guid LockerId { get; init; }
    }

    public class CommandHandler : IRequestHandler<Command, LockerDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CommandHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<LockerDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var locker = await _context.Lockers
                .Include(x => x.Room)
                .FirstOrDefaultAsync(x => x.Id.Equals(request.LockerId), cancellationToken);
            if (locker is null)
            {
                throw new KeyNotFoundException("Locker does not exist.");
            }

            if (locker.IsAvailable)
            {
                throw new ConflictException("Locker has already been enabled.");
            }
        
            locker.IsAvailable = true;
            var result = _context.Lockers.Update(locker);
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<LockerDto>(result.Entity);
        }
    }
}