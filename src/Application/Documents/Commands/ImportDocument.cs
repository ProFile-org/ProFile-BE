using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Messages;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using Domain.Entities.Logging;
using Domain.Entities.Physical;
using Domain.Statuses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Application.Documents.Commands;

public class ImportDocument
{
    public record Command : IRequest<DocumentDto>
    {
        public string Title { get; init; } = null!;
        public string? Description { get; init; }
        public string DocumentType { get; init; } = null!;
        public Guid ImporterId { get; init; }
    }

    public class CommandHandler : IRequestHandler<Command, DocumentDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        private readonly ILogger<CommandHandler> _logger;

        public CommandHandler(IApplicationDbContext context, IMapper mapper, ILogger<CommandHandler> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<DocumentDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var importer = await _context.Users
                .Include(x => x.Department)
                .FirstOrDefaultAsync(x => x.Id == request.ImporterId, cancellationToken);
            if (importer is null)
            {
                throw new UnauthorizedAccessException();
            }

            var document = _context.Documents.FirstOrDefault(x =>
                x.Title.Trim().ToLower().Equals(request.Title.Trim().ToLower())
                && x.Importer != null
                && x.Importer.Id == request.ImporterId);
            if (document is not null)
            {
                throw new ConflictException($"Document title already exists for user {importer.LastName}.");
            }

            var entity = new Document()
            {
                Title = request.Title.Trim(),
                Description = request.Description?.Trim(),
                DocumentType = request.DocumentType.Trim(),
                Importer = importer,
                Department = importer.Department,
                Status = DocumentStatus.Issued,
            };

            var log = new DocumentLog()
            {
                Object = entity,
                Time = LocalDateTime.FromDateTime(DateTime.Now),
                User = importer,
                UserId = importer.Id,
                Action = DocumentLogMessages.Import.NewImport
            };

            var result = await _context.Documents.AddAsync(entity, cancellationToken);
            await _context.DocumentLogs.AddAsync(log, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<DocumentDto>(result.Entity);
        }
    }
}