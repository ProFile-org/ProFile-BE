using Application.Common.Exceptions;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Digital;
using Domain.Entities.Physical;
using Domain.Statuses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Documents.Commands;

public abstract class UploadFileToDocument
{
    public record Command : IRequest<DocumentDto>
    {
        public User CurrentUser { get; init; } = null!;
        public Guid DocumentId { get; init; }
        public MemoryStream FileData { get; init; } = null!;
        public string FileType { get; init; } = null!;
        public string FileExtension { get; init; } = null!;
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
                .Include(x => x.File)
                .Include(x => x.Importer)
                .Include(x => x.Department)
                .Include(x => x.Folder)
                .ThenInclude(y => y!.Locker)
                .ThenInclude(z => z.Room)
                .FirstOrDefaultAsync(x => x.Id == request.DocumentId, cancellationToken);

            if (document is null)
            {
                throw new KeyNotFoundException("Document does not exist.");
            }

            if (request.CurrentUser.Role.IsEmployee() && document.ImporterId != request.CurrentUser.Id)
            {
                throw new UnauthorizedAccessException("User cannot upload file to this document.");
            }
            
            if (request.CurrentUser.Role.IsStaff() && document.Department!.Id != request.CurrentUser.Department!.Id)
            {
                throw new UnauthorizedAccessException("User cannot upload file to this document.");
            }

            if (document.File is not null)
            {
                _context.Files.Remove(document.File);
            }
            
            if (document.Status is DocumentStatus.Lost )
            {
                throw new ConflictException("This document cannot be uploaded to.");
            }
            
            var file = new FileEntity
            {
                FileData = request.FileData.ToArray(),
                FileType = request.FileType!,
                FileExtension = request.FileExtension,
            };
            var fileEntity = await _context.Files.AddAsync(file, cancellationToken);

            document.File = fileEntity.Entity;
            document.FileId = fileEntity.Entity.Id;

            var result = _context.Documents.Update(document);
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<DocumentDto>(result.Entity);
        }
    }
}