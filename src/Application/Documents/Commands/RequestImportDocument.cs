using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Messages;
using Application.Common.Models.Dtos.ImportDocument;
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

public class RequestImportDocument
{
    public record Command : IRequest<IssuedDocumentDto>
    {
        public string Title { get; init; } = null!;
        public string? Description { get; init; }
        public string DocumentType { get; init; } = null!;
        public Guid IssuerId { get; init; }
        public bool IsPrivate { get; set; }
    }

    public class CommandHandler : IRequestHandler<Command, IssuedDocumentDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CommandHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IssuedDocumentDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var issuer = await _context.Users
                .Include(x => x.Department)
                .FirstOrDefaultAsync(x => x.Id == request.IssuerId, cancellationToken);
            if (issuer is null)
            {
                throw new UnauthorizedAccessException();
            }

            var document = _context.Documents.FirstOrDefault(x =>
                x.Title.Trim().ToLower().Equals(request.Title.Trim().ToLower())
                && x.Importer != null
                && x.Importer.Id == request.IssuerId);
            if (document is not null)
            {
                throw new ConflictException($"Document title already exists for user {issuer.LastName}.");
            }

            var entity = new Document()
            {
                Title = request.Title.Trim(),
                Description = request.Description?.Trim(),
                DocumentType = request.DocumentType.Trim(),
                Importer = issuer,
                Department = issuer.Department,
                Status = DocumentStatus.Issued,
                IsPrivate = request.IsPrivate,
                Created = LocalDateTime.FromDateTime(DateTime.Now),
                CreatedBy = issuer.Id,
            };
            var log = new DocumentLog()
            {
                Object = entity,
                Time = LocalDateTime.FromDateTime(DateTime.Now),
                User = issuer,
                UserId = issuer.Id,
                Action = DocumentLogMessages.Import.NewImportRequest,
            };

            var result = await _context.Documents.AddAsync(entity, cancellationToken);
            await _context.DocumentLogs.AddAsync(log, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<IssuedDocumentDto>(result.Entity);
        }
    }
}