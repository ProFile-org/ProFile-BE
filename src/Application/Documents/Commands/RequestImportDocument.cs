using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Messages;
using Application.Common.Models.Dtos.ImportDocument;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using Domain.Entities;
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
        public User Issuer { get; init; } = null!;
        public Guid RoomId { get; init; }
        public bool IsPrivate { get; init; }
    }

    public class CommandHandler : IRequestHandler<Command, IssuedDocumentDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IDateTimeProvider _dateTimeProvider;

        public CommandHandler(IApplicationDbContext context, IMapper mapper, IDateTimeProvider dateTimeProvider)
        {
            _context = context;
            _mapper = mapper;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<IssuedDocumentDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var document = _context.Documents.FirstOrDefault(x =>
                x.Title.Trim().ToLower().Equals(request.Title.Trim().ToLower())
                && x.Importer!.Id == request.Issuer.Id);
            if (document is not null)
            {
                throw new ConflictException($"Document title already exists for user {request.Issuer.LastName}.");
            }

            var localDateTimeNow = LocalDateTime.FromDateTime(_dateTimeProvider.DateTimeNow);

            var entity = new Document()
            {
                Title = request.Title.Trim(),
                Description = request.Description?.Trim(),
                DocumentType = request.DocumentType.Trim(),
                Importer = request.Issuer,
                Department = request.Issuer.Department,
                Status = DocumentStatus.Issued,
                IsPrivate = request.IsPrivate,
                Created = localDateTimeNow,
                CreatedBy = request.Issuer.Id,
            };
            
            var log = new DocumentLog()
            {
                Object = entity,
                Time = localDateTimeNow,
                User = request.Issuer,
                UserId = request.Issuer.Id,
                Action = DocumentLogMessages.Import.NewImportRequest,
            };
            var result = await _context.Documents.AddAsync(entity, cancellationToken);
            await _context.DocumentLogs.AddAsync(log, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<IssuedDocumentDto>(result.Entity);
        }
    }
}