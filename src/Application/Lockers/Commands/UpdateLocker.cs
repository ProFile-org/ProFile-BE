using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using Domain.Entities.Physical;
using Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Lockers.Commands;

public class UpdateLocker
{
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;
            
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(64).WithMessage("Locker's name cannot exceed 64 characters.");
            
            RuleFor(x => x.Description)
                .MaximumLength(256).WithMessage("Locker's description cannot exceed 256 characters.");
            
            RuleFor(x => x.Capacity)
                .GreaterThan(0).WithMessage("Locker's capacity cannot be less than 1");
        }
    }
    public record Command : IRequest<LockerDto>
    {
        public Guid LockerId { get; init; }
        public string Name { get; init; } = null!;
        public string? Description { get; init; }
        public int Capacity { get; init; }
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
            var locker = await _context.Lockers.Include(x => x.Room).FirstOrDefaultAsync(
                x => x.Id.Equals(request.LockerId), cancellationToken);

            if (locker is null)
            {
                throw new KeyNotFoundException("Locker does not exist.");
            }

            var duplicateLocker = await _context.Lockers.FirstOrDefaultAsync(
                x => x.Name.Trim().ToLower().Equals(request.Name.Trim().ToLower()) 
                     && x.Id != locker.Id
                     && x.Room.Id == locker.Room.Id, cancellationToken);
            
            if (duplicateLocker is not null && !duplicateLocker.Equals(locker))
            {
                throw new ConflictException("New locker name already exists.");
            }

            if (locker.NumberOfFolders > request.Capacity)
            {
                throw new ConflictException("New capacity cannot be less than current number of folders.");
            }

            locker.Name = request.Name;
            locker.Description = request.Description;
            locker.Capacity = request.Capacity;

            var result = _context.Lockers.Update(locker);
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<LockerDto>(result.Entity);
        }
    }
}