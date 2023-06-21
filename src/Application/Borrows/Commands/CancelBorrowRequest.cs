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

public class CancelBorrowRequest
{
    public record Command : IRequest<BorrowDto>
    {
        public Guid CurrentUserId { get; init; }
        public Guid BorrowId { get; init; }
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

            if (borrowRequest.Status is not BorrowRequestStatus.Pending)
            {
                throw new ConflictException("Request cannot be cancelled.");
            }

            if (borrowRequest.Borrower.Id != request.CurrentUserId)
            {
                throw new ConflictException("Can not cancel other borrow request");
            }
            
            var localDateTimeNow = LocalDateTime.FromDateTime(_dateTimeProvider.DateTimeNow);

            var currentUser = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == request.CurrentUserId, cancellationToken);
            var log = new DocumentLog()
            {
                ObjectId = borrowRequest.Document.Id,
                UserId = currentUser!.Id,
                User = currentUser,
                Time = localDateTimeNow,
                Action = DocumentLogMessages.Borrow.CanCel,
            };

            borrowRequest.Status = BorrowRequestStatus.Cancelled;
            var result = _context.Borrows.Update(borrowRequest);
            await _context.DocumentLogs.AddAsync(log, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<BorrowDto>(result.Entity);
        }
    }
}