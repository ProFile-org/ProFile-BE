using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Documents.Commands;

public class UpdateDocument
{
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(64).WithMessage("Title cannot exceed 64 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(256).WithMessage("Description cannot exceed 256 characters.");

            RuleFor(x => x.DocumentType)
                .NotEmpty().WithMessage("DocumentType is required.")
                .MaximumLength(64).WithMessage("DocumentType cannot exceed 64 characters.");
        }
    }

    public record Command : IRequest<DocumentDto>
    {
        public Guid DocumentId { get; init; }
        public string Title { get; init; } = null!; 
        public string? Description { get; init; }
        public string DocumentType { get; init; } = null!; 
    }
    
    public class CommandHandler : IRequestHandler<Command, DocumentDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CommandHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<DocumentDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var document = await _context.Documents
                .Include( x => x.Importer)
                .FirstOrDefaultAsync( x => x.Id.Equals(request.DocumentId), cancellationToken);

            if (document is null)
            {
                throw new KeyNotFoundException("Document does not exist.");
            }

            var titleExisted = await _context.Documents
                .Include(x => x.Importer)
                .AnyAsync(x => 
                x.Title.Trim().ToLower().Equals(request.Title.Trim().ToLower())
                && x.Id != document.Id
                && x.Importer!.Id == document.Importer!.Id
                , cancellationToken);

            if (titleExisted)
            {
                throw new ConflictException("Document name already exists for this importer.");
            }

            document.Title = request.Title;
            document.DocumentType = request.DocumentType;
            document.Description = request.Description;

            var result = _context.Documents.Update(document);
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<DocumentDto>(result.Entity);
        }
    }
}