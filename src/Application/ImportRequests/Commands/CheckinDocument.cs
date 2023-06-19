using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Messages;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Logging;
using Domain.Statuses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Application.ImportRequests.Commands;

public class CheckinDocument
{
    public record Command : IRequest<DocumentDto>
    {
        public User CurrentUser { get; init; } = null!;
        public Guid DocumentId { get; init; }
    }

    public class CommandHandler : IRequestHandler<Command, DocumentDto>
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

        public async Task<DocumentDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var document = await _context.Documents
                .Include(x => x.Department)
                .Include(x => x.Folder)
                .FirstOrDefaultAsync(x => x.Id == request.DocumentId, cancellationToken);

            if (document is null)
            {
                throw new KeyNotFoundException("Document does not exist.");
            }

            var importRequest = await _context.ImportRequests
                .Include(x => x.Document)
                .Include(x => x.Room)
                .FirstOrDefaultAsync(x => x.DocumentId == request.DocumentId, cancellationToken);

            if (importRequest is null)
            {
                throw new ConflictException("This document does not have an import request.");
            }

            if (StatusesAreNotValid(document.Status, importRequest.Status))
            {
                throw new ConflictException("Request cannot be checked in.");
            }

            if (document.Folder is null)
            {
                throw new ConflictException("Request cannot be checked in.");
            }

            var localDateTimeNow = LocalDateTime.FromDateTime(_dateTimeProvider.DateTimeNow);
            
            document.Status = DocumentStatus.Available;
            document.LastModified = localDateTimeNow;
            document.LastModifiedBy =  request.CurrentUser.Id;
            importRequest.Status = ImportRequestStatus.CheckedIn;
            importRequest.LastModified = localDateTimeNow;
            importRequest.LastModifiedBy = request.CurrentUser.Id;
            
            var log = new DocumentLog()
            {
                User =  request.CurrentUser,
                UserId =  request.CurrentUser.Id,
                Object = document,
                Time = localDateTimeNow,
                Action = DocumentLogMessages.Import.Checkin,
            };
            var importLog = new RequestLog()
            {
                User = request.CurrentUser,
                UserId = request.CurrentUser.Id,
                Object = document,
                Time = localDateTimeNow,
                Action = RequestLogMessages.CheckInImport,
            };
            var result = _context.Documents.Update(document);
            _context.ImportRequests.Update(importRequest);
            await _context.DocumentLogs.AddAsync(log, cancellationToken);
            await _context.RequestLogs.AddAsync(importLog, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<DocumentDto>(result.Entity);
        }

        private static bool StatusesAreNotValid(DocumentStatus documentStatus, ImportRequestStatus importRequestStatus)
            => documentStatus is not DocumentStatus.Available || importRequestStatus is not ImportRequestStatus.Approved;
    }
}