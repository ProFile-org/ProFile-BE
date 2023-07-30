using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Logging;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using Domain.Statuses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<CancelBorrowRequest> _logger;

        public CommandHandler(IApplicationDbContext context, IMapper mapper, IDateTimeProvider dateTimeProvider, ILogger<CancelBorrowRequest> logger)
        {
            _context = context;
            _mapper = mapper;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
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

            borrowRequest.Status = BorrowRequestStatus.Cancelled;
            var result = _context.Borrows.Update(borrowRequest);
            await _context.SaveChangesAsync(cancellationToken);
            using (Logging.PushProperties("BorrowRequest", borrowRequest.Id, request.CurrentUserId))
            {
                Common.Extensions.Logging.BorrowLogExtensions.LogCancelBorrowRequest(_logger, borrowRequest.Document.Id.ToString(), borrowRequest.Id.ToString());
            }
            return _mapper.Map<BorrowDto>(result.Entity);
        }
    }
}