using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Logging;
using Application.Common.Messages;
using Application.Common.Models.Dtos.ImportDocument;
using Application.Common.Models.Dtos.Physical;
using Application.ImportRequests;
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

public class ImportDocument
{
    public record Command : IRequest<DocumentDto>
    {
        public User CurrentUser { get; init; } = null!;
        public Guid? CurrentStaffRoomId { get; init; }
        public string Title { get; init; } = null!;
        public string? Description { get; init; }
        public string DocumentType { get; init; } = null!;
        public Guid ImporterId { get; init; }
        public Guid FolderId { get; init; }
        public bool IsPrivate { get; init; }
    }

    public class CommandHandler : IRequestHandler<Command, DocumentDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger<ImportDocument> _logger;

        public CommandHandler(IApplicationDbContext context, IMapper mapper, IDateTimeProvider dateTimeProvider, ILogger<ImportDocument> logger)
        {
            _context = context;
            _mapper = mapper;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
        }

        public async Task<DocumentDto> Handle(Command request, CancellationToken cancellationToken)
        {
            if (request.CurrentStaffRoomId is null)
            {
                throw new UnauthorizedAccessException("User cannot access this resource.");
            }
            
            var importer = await _context.Users
                .Include(x => x.Department)
                .FirstOrDefaultAsync(x => x.Id == request.ImporterId, cancellationToken);
            if (importer is null)
            {
                throw new KeyNotFoundException("User does not exist.");
            }

            if (importer.Department is null)
            {
                throw new ConflictException("User does not have a department.");
            }

            if (importer.Department.Id != request.CurrentUser.Department!.Id)
            {
                throw new ConflictException("User is in another department as staff.");
            }

            var document = _context.Documents.FirstOrDefault(x =>
                x.Title.Trim().ToLower().Equals(request.Title.Trim().ToLower())
                && x.Importer != null
                && x.Importer.Id == request.ImporterId);
            if (document is not null)
            {
                throw new ConflictException($"Document title already exists for user {importer.LastName}.");
            }

            var folder = await _context.Folders
                .Include(x => x.Locker)
                .ThenInclude(y => y.Room)
                .FirstOrDefaultAsync(x => x.Id == request.FolderId, cancellationToken);
            if (folder is null)
            {
                throw new KeyNotFoundException("Folder does not exist.");
            }

            if (folder.Capacity == folder.NumberOfDocuments)
            {
                throw new ConflictException("This folder cannot accept more documents.");
            }

            if (folder.Locker.Room.Id != request.CurrentStaffRoomId)
            {
                throw new UnauthorizedAccessException("User cannot access this resource.");
            }

            var localDateTimeNow = LocalDateTime.FromDateTime(_dateTimeProvider.DateTimeNow);
            
            var entity = new Document()
            {
                Title = request.Title.Trim(),
                Description = request.Description?.Trim(),
                DocumentType = request.DocumentType.Trim(),
                Importer = importer,
                Department = importer.Department,
                Folder = folder,
                Status = DocumentStatus.Available,
                IsPrivate = request.IsPrivate,
                Created = localDateTimeNow,
                CreatedBy = request.CurrentUser.Id,
            };
            var log = new DocumentLog()
            {
                User = request.CurrentUser,
                UserId = request.CurrentUser.Id,
                ObjectId = entity.Id,
                Time = localDateTimeNow,
                Action = DocumentLogMessages.Import.NewImport,
            };

            var result = await _context.Documents.AddAsync(entity, cancellationToken);
            folder.NumberOfDocuments += 1;
            _context.Folders.Update(folder);
            await _context.DocumentLogs.AddAsync(log, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            using (Logging.PushProperties(nameof(Document), result.Entity.Id, request.CurrentUser.Id))
            {
                _logger.LogImportDocument(result.Entity.Id.ToString());
            }
            using (Logging.PushProperties(nameof(Folder), folder.Id, request.CurrentUser.Id))
            {
                _logger.LogAssignDocumentToFolder(result.Entity.Id.ToString());
            }
            return _mapper.Map<DocumentDto>(result.Entity);
        }
    }
}