using Application.Common.Exceptions;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Messages;
using Application.Common.Models.Dtos.ImportDocument;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Logging;
using Domain.Statuses;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Application.ImportRequests.Commands;

public class ApproveOrRejectDocument
{
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Decision)
                .Must(x => x.IsApproval() || x.IsRejection()).WithMessage("Decision is not valid.");
        }
    }

    public record Command : IRequest<ImportRequestDto>
    {
        public User CurrentUser { get; init; } = null!;
        public Guid ImportRequestId { get; init; }
        public string Decision { get; init; } = null!;
        public string Reason { get; init; } = null!;
    }

    public class CommandHandler : IRequestHandler<Command, ImportRequestDto>
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

        public async Task<ImportRequestDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var importRequest = await _context.ImportRequests
                .Include(x => x.Document)
                .Include(x => x.Room)
                .FirstOrDefaultAsync(x => x.Id == request.ImportRequestId
                                          && x.Status == ImportRequestStatus.Pending, cancellationToken);

            if (importRequest is null)
            {
                throw new KeyNotFoundException("Import request does not exist.");
            }

            var document = await _context.Documents
                .Include(x => x.Department)
                .FirstOrDefaultAsync(x => x.Id == importRequest.Document.Id
                                          && x.Status == DocumentStatus.Issued, cancellationToken);
            if (document is null)
            {
                throw new ConflictException("Document does not exist.");
            }

            var localDateTimeNow = LocalDateTime.FromDateTime(_dateTimeProvider.DateTimeNow);

            var log = new DocumentLog()
            {
                ObjectId = document.Id,
                Time = localDateTimeNow,
                User = request.CurrentUser,
                UserId = request.CurrentUser.Id,
                Action = DocumentLogMessages.Import.Approve,
            };

            var requestLog = new RequestLog()
            {
                ObjectId = document.Id,
                Time = localDateTimeNow,
                User = request.CurrentUser,
                UserId = request.CurrentUser.Id,
                Action = RequestLogMessages.ApproveImport,
            };

            if (request.Decision.IsApproval())
            {
                importRequest.Status = ImportRequestStatus.Approved;
                log.Action = DocumentLogMessages.Import.Approve;
                requestLog.Action = RequestLogMessages.ApproveImport;
            }

            if (request.Decision.IsRejection())
            {
                importRequest.Status = ImportRequestStatus.Rejected;
                log.Action = DocumentLogMessages.Import.Reject;
                requestLog.Action = RequestLogMessages.RejectImport;
            }

            importRequest.Reason = request.Reason;
            importRequest.LastModified = localDateTimeNow;
            importRequest.LastModifiedBy = request.CurrentUser.Id;

            var result = _context.ImportRequests.Update(importRequest);
            await _context.DocumentLogs.AddAsync(log, cancellationToken);
            await _context.RequestLogs.AddAsync(requestLog, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<ImportRequestDto>(result.Entity);
        }
    }
}