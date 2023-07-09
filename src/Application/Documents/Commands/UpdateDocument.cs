using Application.Common.Exceptions;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Logging;
using Application.Common.Messages;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Logging;
using Domain.Entities.Physical;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NodaTime;

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
        public User CurrentUser { get; init; } = null!; 
        public Guid DocumentId { get; init; }
        public string Title { get; init; } = null!; 
        public string? Description { get; init; }
        public string DocumentType { get; init; } = null!; 
        public bool IsPrivate { get; init; }
    }
    
    public class CommandHandler : IRequestHandler<Command, DocumentDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger<UpdateDocument> _logger;

        public CommandHandler(IApplicationDbContext context, IMapper mapper, IDateTimeProvider dateTimeProvider, ILogger<UpdateDocument> logger)
        {
            _context = context;
            _mapper = mapper;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
        }

        public async Task<DocumentDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var document = await _context.Documents
                .Include(x => x.Department)
                .Include( x => x.Importer)
                .FirstOrDefaultAsync( x => x.Id == request.DocumentId, cancellationToken);

            if (document is null)
            {
                throw new KeyNotFoundException("Document does not exist.");
            }
            
            if (ViolateConstraints(request.CurrentUser, document))
            {
                throw new UnauthorizedAccessException("Cannot update this document.");
            }

            if (document.Importer is not null)
            {
                var titleExisted = await _context.Documents
                    .Include(x => x.Importer)
                    .AnyAsync(x => 
                        x.Title.Trim().ToLower().Equals(request.Title.Trim().ToLower())
                        && x.Id != document.Id
                        && x.Importer!.Id == document.Importer!.Id, cancellationToken);

                if (titleExisted)
                {
                    throw new ConflictException("Document name already exists for this importer.");
                }    
            }
            
            var localDateTimeNow = LocalDateTime.FromDateTime(_dateTimeProvider.DateTimeNow);

            document.Title = request.Title;
            document.DocumentType = request.DocumentType;
            document.Description = request.Description;
            document.IsPrivate = request.IsPrivate;
            document.LastModified = localDateTimeNow;
            document.LastModifiedBy = request.CurrentUser.Id;

            var log = new DocumentLog()
            {
                ObjectId = document.Id,
                Time = localDateTimeNow,
                User = request.CurrentUser,
                UserId = request.CurrentUser.Id,
                Action = DocumentLogMessages.Update,
            };
            var result = _context.Documents.Update(document);
            await _context.DocumentLogs.AddAsync(log, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            using (Logging.PushProperties(nameof(Document), result.Entity.Id, request.CurrentUser.Id))
            {
                _logger.LogUpdateDocument(result.Entity.Id.ToString());
            }

            return _mapper.Map<DocumentDto>(result.Entity);
        }

        private static bool ViolateConstraints(User currentUser, Document document)
            => (currentUser.Role.IsStaff()
                && currentUser.Department!.Id != document.Department!.Id)
            || (currentUser.Role.IsEmployee()
                && document.ImporterId != currentUser.Id);
    }
}