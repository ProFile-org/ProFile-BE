using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Messages;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using Domain.Entities.Logging;
using Domain.Statuses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Application.Documents.Commands;

public class AssignDocument
{
    public record Command : IRequest<DocumentDto>
    {
        public Guid PerformingUserId { get; init; }
        public Guid DocumentId { get; init; }
        public Guid FolderId { get; init; }
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
                .Include(x => x.Folder)
                .FirstOrDefaultAsync(x =>
                x.Id == request.DocumentId, cancellationToken);
            if (document is null)
            {
                throw new ConflictException("Document does not exist.");
            }

            if (document.Status is not DocumentStatus.Approved)
            {
                throw new ConflictException("Document cannot be assigned.");
            }

            var folder = await _context.Folders
                .FirstOrDefaultAsync(x => x.Id == request.FolderId, cancellationToken);

            if (folder is null)
            {
                throw new ConflictException("Folder does not exist.");
            }

            if (folder.NumberOfDocuments >= folder.Capacity)
            {
                throw new ConflictException("This folder cannot accept more documents.");
            }

            var performingUser = await _context.Users.FirstOrDefaultAsync(x => x.Id == request.PerformingUserId, cancellationToken);
            document.Folder = folder;
            document.LastModified = LocalDateTime.FromDateTime(DateTime.Now);
            document.LastModifiedBy = performingUser!.Id;
            var log = new DocumentLog()
            {
                Object = document,
                Time = LocalDateTime.FromDateTime(DateTime.Now),
                User = performingUser!,
                UserId = performingUser!.Id,
                Action = DocumentLogMessages.Import.Assign,
            };
            var folderLog = new FolderLog()
            {
                Object = folder,
                Time = LocalDateTime.FromDateTime(DateTime.Now),
                User = performingUser!,
                UserId = performingUser!.Id,
                Action = FolderLogMessage.AssignDocument,
            };
            var result = _context.Documents.Update(document);
            await _context.DocumentLogs.AddAsync(log, cancellationToken);
            await _context.FolderLogs.AddAsync(folderLog, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<DocumentDto>(result.Entity);
        }
    }
}