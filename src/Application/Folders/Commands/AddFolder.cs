using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using Domain.Entities.Physical;
using Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Folders.Commands;

public class AddFolder
{
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(f => f.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(64).WithMessage("Name cannot exceed 64 characters.");

            RuleFor(f => f.Description)
                .MaximumLength(256).WithMessage("Description cannot exceed 256 characters.");

            RuleFor(f => f.Capacity)
                .NotEmpty().WithMessage("Folder capacity is required.")
                .GreaterThanOrEqualTo(1).WithMessage("Folder's capacity cannot be less than 1.");

            RuleFor(f => f.LockerId)
                .NotEmpty().WithMessage("LockerId is required.");
        }
    }

    public record Command : IRequest<FolderDto>
    {
        public string Name { get; init; } = null!;
        public string? Description { get; init; }
        public int Capacity { get; init; }
        public Guid LockerId { get; init; }
    }

    public class CommandHandler : IRequestHandler<Command, FolderDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CommandHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<FolderDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var locker = await _context.Lockers.FirstOrDefaultAsync(l => l.Id == request.LockerId, cancellationToken);

            if (locker is null)
            {
                throw new KeyNotFoundException("Locker does not exist.");
            }

            if (locker.NumberOfFolders >= locker.Capacity)
            {
                throw new LimitExceededException("This locker cannot accept more folders.");
            }

            var folder = await _context.Folders.FirstOrDefaultAsync(x =>
                x.Name.Trim().ToLower().Equals(request.Name.Trim().ToLower())
                && x.Locker.Id.Equals(request.LockerId), cancellationToken);

            if (folder is not null)
            {
                throw new ConflictException("Folder's name already exists.");
            }

            var entity = new Folder
            {
                Name = request.Name.Trim(),
                Description = request.Description,
                NumberOfDocuments = 0,
                Capacity = request.Capacity,
                Locker = locker,
                IsAvailable = true
            };
            var result = await _context.Folders.AddAsync(entity, cancellationToken);
            locker.NumberOfFolders += 1;
            _context.Lockers.Update(locker);
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<FolderDto>(result.Entity);
        }
    }
}