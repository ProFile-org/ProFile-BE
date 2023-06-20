using Application.Common.Exceptions;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Messages;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using Domain.Entities.Logging;
using Domain.Enums;
using Domain.Statuses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Application.Borrows.Commands;

public class ApproveOrRejectBorrowRequest
{
    public record Command : IRequest<BorrowDto>
    {
        public Guid CurrentUserId { get; init; }
        public Guid BorrowId { get; init; }
        public string Decision { get; init; } = null!;
        public string Reason { get; init; } = null!;
    }

    public class CommandHandler : IRequestHandler<Command, BorrowDto>
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

        public async Task<BorrowDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var borrowRequest = await _context.Borrows
                .Include(x => x.Borrower)
                .Include(x => x.Document)
                .FirstOrDefaultAsync(x => x.Id == request.BorrowId, cancellationToken);
            if (borrowRequest is null)
            {
                throw new KeyNotFoundException("Borrow request does not exist.");
            }

            if (borrowRequest.Document.Status is DocumentStatus.Lost)
            {
                borrowRequest.Status = BorrowRequestStatus.NotProcessable;
                _context.Borrows.Update(borrowRequest);
                await _context.SaveChangesAsync(cancellationToken);
                throw new ConflictException("Document is lost. Request is unprocessable.");
            }

            if (borrowRequest.Status is not (BorrowRequestStatus.Pending or BorrowRequestStatus.Rejected)
                && request.Decision.IsApproval())
            {
                throw new ConflictException("Request cannot be approved.");
            }

            if (borrowRequest.Status is not BorrowRequestStatus.Pending
                && request.Decision.IsRejection())
            {
                throw new ConflictException("Request cannot be rejected.");
            }

            var currentUser = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == request.CurrentUserId, cancellationToken);

            var localDateTimeNow = LocalDateTime.FromDateTime(_dateTimeProvider.DateTimeNow);

            var existedBorrows =  _context.Borrows
                .Where(x =>
                    x.Document.Id == borrowRequest.Document.Id
                    && x.Id != borrowRequest.Id
                    && (x.DueTime > localDateTimeNow
                        || x.Status == BorrowRequestStatus.Overdue));

            var log = new DocumentLog()
            {
                ObjectId = borrowRequest.Document.Id,
                UserId = currentUser!.Id,
                User = currentUser,
                Time = localDateTimeNow,
                Action = DocumentLogMessages.Borrow.Approve,
            };
            var requestLog = new RequestLog()
            {
                ObjectId = borrowRequest.Document.Id,
                Type = RequestType.Borrow,
                UserId = currentUser.Id,
                User = currentUser,
                Time = localDateTimeNow,
                Action = RequestLogMessages.ApproveBorrow,
            };

            if (request.Decision.IsApproval())
            {
                foreach (var existedBorrow in existedBorrows)
                {
                    if (existedBorrow.Status
                        is BorrowRequestStatus.Approved
                        or BorrowRequestStatus.CheckedOut
                        && borrowRequest.BorrowTime <= existedBorrow.DueTime && borrowRequest.DueTime >= existedBorrow.BorrowTime)
                    {
                        throw new ConflictException("Request cannot be approved.");
                    }
                }

                borrowRequest.Status = BorrowRequestStatus.Approved;
            }

            if (request.Decision.IsRejection())
            {
                borrowRequest.Status = BorrowRequestStatus.Rejected;
                log.Action = DocumentLogMessages.Borrow.Reject;
                requestLog.Action = RequestLogMessages.RejectBorrow;
            }

            borrowRequest.Reason = request.Reason;
            borrowRequest.LastModified = localDateTimeNow;
            borrowRequest.LastModifiedBy = currentUser.Id;
            
            var result = _context.Borrows.Update(borrowRequest);
            await _context.DocumentLogs.AddAsync(log, cancellationToken);
            await _context.RequestLogs.AddAsync(requestLog, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<BorrowDto>(result.Entity);
        }
    }
}