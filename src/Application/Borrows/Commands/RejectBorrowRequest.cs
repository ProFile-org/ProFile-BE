using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Messages;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using Domain.Entities.Logging;
using Domain.Statuses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace Application.Borrows.Commands;

public class RejectBorrowRequest
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

            if (borrowRequest.Status is not BorrowRequestStatus.Pending)
            {
                throw new ConflictException("Request cannot be rejected.");
            }

            var performingUser = await _context.Users.FirstOrDefaultAsync(x => x.Id == request.PerformingUserId, cancellationToken);
            borrowRequest.Status = BorrowRequestStatus.Rejected;
            var requestLog = new RequestLog()
            {
                ObjectId = borrowRequest.Document.Id,
                UserId = performingUser!.Id,
                User = performingUser,
                Time = LocalDateTime.FromDateTime(DateTime.Now),
                Action = DocumentLogMessages.Borrow.Reject,
            };
            var result = _context.Borrows.Update(borrowRequest);
            await _context.RequestLogs.AddAsync(requestLog, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<BorrowDto>(result.Entity);
        }
    }
}