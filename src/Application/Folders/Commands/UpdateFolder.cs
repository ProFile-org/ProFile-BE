using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Folders.Commands;

public class UpdateFolder
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
        }
    }
    
    public record Command : IRequest<FolderDto>
    {
        public Guid FolderId { get; init; }
        public string Name { get; init; } = null!;
        public string? Description { get; init; }
        public int Capacity { get; init; }
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
            var folder = await _context.Folders
                .FirstOrDefaultAsync(x => x.Id.Equals(request.FolderId), cancellationToken);

            if (folder is null)
            {
                throw new KeyNotFoundException("Folder does not exist.");
            }

            var nameExisted = await _context.Folders.AnyAsync( x => 
                    x.Name.Trim().ToLower().Equals(request.Name.Trim().ToLower()) 
                    && x.Id != folder.Id
                    && x.Locker.Id == folder.Locker.Id
                    , cancellationToken);

            if (nameExisted)
            {
                throw new ConflictException("Folder name already exists.");
            }

            if (request.Capacity < folder.NumberOfDocuments)
            {
                throw new ConflictException("New capacity cannot be less than current number of documents.");
            }

            folder.Name = request.Name;
            folder.Description = request.Description;
            folder.Capacity = request.Capacity;

            var result = _context.Folders.Update(folder);
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<FolderDto>(result.Entity);
        }
    }
}