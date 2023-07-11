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

public class AssignDocument
{
    public record Command : IRequest<ImportRequestDto>
    {
        public User CurrentUser { get; init; } = null!;
        public Guid? StaffRoomId { get; init; }
        public Guid ImportRequestId { get; init; }
        public Guid FolderId { get; init; }
    }

    public class CommandHandler : IRequestHandler<Command, ImportRequestDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger<AssignDocument> _logger;

        public CommandHandler(IApplicationDbContext context, IMapper mapper, IDateTimeProvider dateTimeProvider, ILogger<AssignDocument> logger)
        {
            _context = context;
            _mapper = mapper;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
        }

        public async Task<ImportRequestDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var importRequest = await _context.ImportRequests
                .Include(x => x.Document)
                .Include(x => x.Room)
                .FirstOrDefaultAsync(x => x.Id == request.ImportRequestId, cancellationToken);

            if (importRequest is null)
            {
                throw new KeyNotFoundException("Import request does not exist.");
            }

            if (request.StaffRoomId is null || importRequest.RoomId != request.StaffRoomId.Value)
            {
                throw new UnauthorizedAccessException("User cannot access this resource.");
            }

            if (importRequest.Status is not ImportRequestStatus.Approved)
            {
                throw new ConflictException("Request cannot be assigned.");
            }

            var folder = await _context.Folders
                .FirstOrDefaultAsync(x => x.Id == request.FolderId
                                    && x.Locker.Room.Id == request.StaffRoomId, cancellationToken);

            if (folder is null)
            {
                throw new ConflictException("Folder does not exist.");
            }

            if (folder.NumberOfDocuments >= folder.Capacity)
            {
                throw new ConflictException("This folder cannot accept more documents.");
            }

            var localDateTimeNow = LocalDateTime.FromDateTime(_dateTimeProvider.DateTimeNow);

            importRequest.Document.Folder = folder;
            importRequest.Document.LastModified = localDateTimeNow;
            importRequest.Document.LastModifiedBy = request.CurrentUser.Id;
            importRequest.Status = ImportRequestStatus.Assigned;
          
            folder.NumberOfDocuments += 1;
            folder.LastModified = localDateTimeNow;
            folder.LastModifiedBy = request.CurrentUser.Id;

            var log = new DocumentLog()
            {
                ObjectId = importRequest.Document.Id,
                Time = localDateTimeNow,
                User = request.CurrentUser,
                UserId = request.CurrentUser.Id,
                Action = DocumentLogMessages.Import.Assign,
            };
            var folderLog = new FolderLog()
            {
                ObjectId = folder.Id,
                Time = localDateTimeNow,
                User = request.CurrentUser,
                UserId = request.CurrentUser.Id,
                Action = FolderLogMessage.AssignDocument,
            };
            _context.Documents.Update(importRequest.Document);
            var result = _context.ImportRequests.Update(importRequest);
            _context.Folders.Update(folder);
            await _context.DocumentLogs.AddAsync(log, cancellationToken);
            await _context.FolderLogs.AddAsync(folderLog, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            using (Logging.PushProperties(nameof(Document), importRequest.Document.Id, request.CurrentUser.Id))
            {
                _logger.LogAssignDocument(importRequest.Document.Id.ToString(), folder.Id.ToString());
            }
            using (Logging.PushProperties(nameof(Folder), folder.Id, request.CurrentUser.Id))
            {
                _logger.LogAssignDocumentToFolder(importRequest.Document.Id.ToString());
            }
            return _mapper.Map<ImportRequestDto>(result.Entity);
        }
    }
}