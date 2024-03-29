using Application.Common.Exceptions;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Logging;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using Domain.Enums;
using Domain.Statuses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Application.Borrows.Commands;

public class ApproveOrRejectBorrowRequest
{
    public record Command : IRequest<BorrowDto>
    {
        public Guid CurrentUserId { get; init; }
        public Guid BorrowId { get; init; }
        public string Decision { get; init; } = null!;
        public string StaffReason { get; init; } = null!;
    }

    public class CommandHandler : IRequestHandler<Command, BorrowDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger<ApproveOrRejectBorrowRequest> _logger;

        public CommandHandler(IApplicationDbContext context, IMapper mapper, IDateTimeProvider dateTimeProvider, ILogger<ApproveOrRejectBorrowRequest> logger)
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
                .ThenInclude(x => x.Folder!)
                .ThenInclude(x => x.Locker)
                .ThenInclude(x => x.Room)
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

            var staff = await _context.Staffs
                .Include(x => x.Room)
                .FirstOrDefaultAsync(x => x.Id == request.CurrentUserId, cancellationToken);

            if (staff is null)
            {
                throw new KeyNotFoundException("Staff does not exist.");
            }

            if (staff.Room is null)
            {
                throw new ConflictException("Staff does not manage a room.");
            }

            if (staff.Room.Id != borrowRequest.Document.Folder!.Locker.Room.Id)
            {
                throw new ConflictException("Request cannot be checked out due to different room.");
            }
            
            var localDateTimeNow = LocalDateTime.FromDateTime(_dateTimeProvider.DateTimeNow);

            var existedBorrows =  _context.Borrows
                .Where(x =>
                    x.Document.Id == borrowRequest.Document.Id
                    && x.Id != borrowRequest.Id
                    && (x.DueTime > localDateTimeNow
                        || x.Status == BorrowRequestStatus.Overdue));

            if (request.Decision.IsApproval())
            {
                foreach (var existedBorrow in existedBorrows)
                {
                    if ((existedBorrow.Status
                        is BorrowRequestStatus.Approved
                        or BorrowRequestStatus.CheckedOut)
                        && (borrowRequest.BorrowTime <= existedBorrow.DueTime && borrowRequest.DueTime >= existedBorrow.BorrowTime))
                    {
                        throw new ConflictException("Request cannot be approved.");
                    }
                }

                borrowRequest.Status = BorrowRequestStatus.Approved;

                using (Logging.PushProperties("BorrowRequest", borrowRequest.Id, request.CurrentUserId))
                {
                    Common.Extensions.Logging.BorrowLogExtensions.LogApproveBorrowRequest(_logger, borrowRequest.Document.Id.ToString(), borrowRequest.Id.ToString());
                }
            }

            if (request.Decision.IsRejection())
            {
                borrowRequest.Status = BorrowRequestStatus.Rejected;

                using (Logging.PushProperties("BorrowRequest", borrowRequest.Id, request.CurrentUserId))
                {
                    Common.Extensions.Logging.BorrowLogExtensions.LogRejectBorrowRequest(_logger, borrowRequest.Document.Id.ToString(), borrowRequest.Id.ToString());
                }
            }

            borrowRequest.StaffReason = request.StaffReason;
            borrowRequest.LastModified = localDateTimeNow;
            borrowRequest.LastModifiedBy = request.CurrentUserId;
            
            var result = _context.Borrows.Update(borrowRequest);
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<BorrowDto>(result.Entity);
        }
    }
}