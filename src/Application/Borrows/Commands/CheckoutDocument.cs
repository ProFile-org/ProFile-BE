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

public class CheckoutDocument
{
    public record Command : IRequest<BorrowDto>
    {
        public Guid PerformingUserId { get; init; }
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

            if (borrowRequest.Document.Status is not DocumentStatus.Available)
            {
                throw new ConflictException("Document is not available.");
            }

            if (borrowRequest.Status is not BorrowRequestStatus.Approved)
            {
                throw new ConflictException("Request cannot be checked out.");
            }

            var performingUser = await _context.Users.FirstOrDefaultAsync(x => x.Id == request.PerformingUserId, cancellationToken);
            borrowRequest.Status = BorrowRequestStatus.CheckedOut;
            borrowRequest.Document.Status = DocumentStatus.Borrowed;
            borrowRequest.Document.LastModified = LocalDateTime.FromDateTime(DateTime.Now);
            borrowRequest.Document.LastModifiedBy = performingUser!.Id;
            var log = new DocumentLog()
            {
                Object = borrowRequest.Document,
                UserId = performingUser.Id,
                User = performingUser,
                Time = LocalDateTime.FromDateTime(DateTime.Now),
                Action = DocumentLogMessages.Borrow.Checkout,
            };
            var result = _context.Borrows.Update(borrowRequest);
            _context.Documents.Update(borrowRequest.Document);
            await _context.DocumentLogs.AddAsync(log, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<BorrowDto>(result.Entity);
        }
    }
}