using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Folders.Commands;

public class DisableFolder
{
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(f => f.FolderId)
                .NotEmpty().WithMessage("FolderId is required.");
        }
    }
    
    public record Command : IRequest<FolderDto>
    {
        public Guid FolderId { get; init; }
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
                .FirstOrDefaultAsync(f => f.Id.Equals(request.FolderId), cancellationToken);

            if (folder is null)
            {
                throw new KeyNotFoundException("Folder does not exist.");
            }

            if (!folder.IsAvailable)
            {
                throw new ConflictException("Folder has already been disabled.");
            }
        
            if (folder.NumberOfDocuments > 0)
            {
                throw new InvalidOperationException("Folder cannot be disabled because it contains documents.");
            }

            folder.IsAvailable = false;
            _context.Folders.Update(folder);
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<FolderDto>(folder);
        }
    }
}