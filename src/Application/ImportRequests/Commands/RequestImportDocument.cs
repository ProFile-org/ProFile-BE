using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Logging;
using Application.Common.Messages;
using Application.Common.Models.Dtos.ImportDocument;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Logging;
using Domain.Entities.Physical;
using Domain.Statuses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Application.ImportRequests.Commands;

public class RequestImportDocument
{
    public record Command : IRequest<ImportRequestDto>
    {
        public string Title { get; init; } = null!;
        public string? Description { get; init; }
        public string DocumentType { get; init; } = null!;
        public string ImportReason { get; init; } = null!;
        public User Issuer { get; init; } = null!;
        public Guid RoomId { get; init; }
        public bool IsPrivate { get; init; }
    }

    public class CommandHandler : IRequestHandler<Command, ImportRequestDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger<RequestImportDocument> _logger;

        public CommandHandler(IApplicationDbContext context, IMapper mapper, IDateTimeProvider dateTimeProvider, ILogger<RequestImportDocument> logger)
        {
            _context = context;
            _mapper = mapper;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
        }

        public async Task<ImportRequestDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var document = _context.Documents.FirstOrDefault(x =>
                x.Title.Trim().ToLower().Equals(request.Title.Trim().ToLower())
                && x.Importer!.Id == request.Issuer.Id);
            if (document is not null)
            {
                throw new ConflictException($"Document title already exists for user {request.Issuer.FirstName}.");
            }

            var room = await _context.Rooms
                .FirstOrDefaultAsync(x => x.Id == request.RoomId && x.IsAvailable, cancellationToken);
            
            if (room is null)
            {
                throw new KeyNotFoundException("Room does not exist.");
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
            await _context.Documents.AddAsync(entity, cancellationToken);
           
            var importRequest = new ImportRequest()
            {
                Document = entity,
                Status = ImportRequestStatus.Pending,
                Room = room,
                Created = localDateTimeNow,
                CreatedBy = request.Issuer.Id,
                ImportReason = request.ImportReason,
                StaffReason = string.Empty,
            };
            
            var log = new DocumentLog()
            {
                ObjectId = entity.Id,
                Time = localDateTimeNow,
                User = request.Issuer,
                UserId = request.Issuer.Id,
                Action = DocumentLogMessages.Import.NewImportRequest,
            };
            var result = await _context.ImportRequests.AddAsync(importRequest, cancellationToken);
            var documentEntry = await _context.DocumentLogs.AddAsync(log, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            using (Logging.PushProperties(nameof(Document), documentEntry.Entity.Id, request.Issuer.Id))
            {
                _logger.LogAddDocument(documentEntry.Entity.Id.ToString());
            }
            using (Logging.PushProperties(nameof(ImportRequest), importRequest.Id, request.Issuer.Id))
            {
                _logger.LogAddDocumentRequest(result.Entity.Id.ToString());
            }
            return _mapper.Map<ImportRequestDto>(result.Entity);
        }
    }
}