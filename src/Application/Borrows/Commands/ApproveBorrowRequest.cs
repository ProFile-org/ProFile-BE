using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using Domain.Statuses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Application.Borrows.Commands;

public class ApproveBorrowRequest
{
    public record Command : IRequest<BorrowDto>
    {
        public Guid BorrowId { get; init; }
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
                    && ((x.DueTime > localDateTimeNow
                         && x.BorrowTime < localDateTimeNow)
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
            
            borrowRequest.Status = BorrowRequestStatus.Approved;
            var result = _context.Borrows.Update(borrowRequest);
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<BorrowDto>(result.Entity);
        }
    }
}