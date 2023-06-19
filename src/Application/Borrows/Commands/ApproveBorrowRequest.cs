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

namespace Application.Borrows.Commands;

public class ApproveBorrowRequest
{
    public record Command : IRequest<BorrowDto>
    {
        public Guid PerformingUserId { get; init; }
        public Guid BorrowId { get; init; }
        public string Reason { get; init; } = null!;
    }

    public class CommandHandler : IRequestHandler<Command, BorrowDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CommandHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
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

            if (borrowRequest.Status is not BorrowRequestStatus.Pending 
                && borrowRequest.Status is not BorrowRequestStatus.Rejected)
            {
                throw new ConflictException("Request cannot be approved.");
            }

            var localDateTimeNow = LocalDateTime.FromDateTime(DateTime.Now);
            var existedBorrow = await _context.Borrows
                .FirstOrDefaultAsync(x =>
                    x.Document.Id == borrowRequest.Document.Id
                    && x.Id != borrowRequest.Id
                    && ((x.DueTime > localDateTimeNow)
                        || x.Status == BorrowRequestStatus.Overdue), cancellationToken);

            if (existedBorrow is not null)
            {
                if (existedBorrow?.Status
                        is BorrowRequestStatus.Approved
                        or BorrowRequestStatus.CheckedOut
                    && borrowRequest.BorrowTime < existedBorrow.DueTime)
                {
                    throw new ConflictException("This document cannot be borrowed.");
                }
            }
            
            var performingUser = await _context.Users.FirstOrDefaultAsync(x => x.Id == request.PerformingUserId, cancellationToken);
            borrowRequest.Status = BorrowRequestStatus.Approved;
            var log = new DocumentLog()
            {
                Object = borrowRequest.Document,
                UserId = performingUser!.Id,
                User = performingUser,
                Time = LocalDateTime.FromDateTime(DateTime.Now),
                Action = DocumentLogMessages.Borrow.Approve,
            };
            var requestLog = new RequestLog()
            {
                Object = borrowRequest.Document,
                UserId = performingUser.Id,
                User = performingUser,
                Time = LocalDateTime.FromDateTime(DateTime.Now),
                Action = DocumentLogMessages.Borrow.Approve,
            };
            var result = _context.Borrows.Update(borrowRequest);
            await _context.DocumentLogs.AddAsync(log, cancellationToken);
            await _context.RequestLogs.AddAsync(requestLog, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<BorrowDto>(result.Entity);
        }
    }
}