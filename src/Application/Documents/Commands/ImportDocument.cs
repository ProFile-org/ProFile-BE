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
using NodaTime;

namespace Application.Documents.Commands;

public class ImportDocument
{
    public record Command : IRequest<DocumentDto>
    {
        public Guid PerformingUserId { get; init; }
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

        public CommandHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<DocumentDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var importer = await _context.Users
                .Include(x => x.Department)
                .FirstOrDefaultAsync(x => x.Id == request.ImporterId, cancellationToken);
            if (importer is null)
            {
                throw new KeyNotFoundException("User does not exist.");
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
                .FirstOrDefaultAsync(x => x.Id == request.FolderId, cancellationToken);
            if (folder is null)
            {
                throw new KeyNotFoundException("Folder does not exist.");
            }

            if (folder.Capacity == folder.NumberOfDocuments)
            {
                throw new ConflictException("This folder cannot accept more documents.");
            }

            var performingUser = await _context.Users.FirstOrDefaultAsync(x => x.Id == request.PerformingUserId, cancellationToken);
            var entity = new Document()
            {
                Title = request.Title.Trim(),
                Description = request.Description?.Trim(),
                DocumentType = request.DocumentType.Trim(),
                Importer = importer,
                Department = importer.Department,
                Folder = folder,
                Status = DocumentStatus.Issued,
                IsPrivate = request.IsPrivate,
                Created = LocalDateTime.FromDateTime(DateTime.Now),
                CreatedBy = performingUser!.Id,
            };
            var log = new DocumentLog()
            {
                User = performingUser,
                UserId = performingUser.Id,
                Object = entity,
                Time = LocalDateTime.FromDateTime(DateTime.Now),
                Action = DocumentLogMessages.Import.NewImport,
            };

            var result = await _context.Documents.AddAsync(entity, cancellationToken);
            folder.NumberOfDocuments += 1;
            _context.Folders.Update(folder);
            await _context.DocumentLogs.AddAsync(log, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<DocumentDto>(result.Entity);
        }
    }
}