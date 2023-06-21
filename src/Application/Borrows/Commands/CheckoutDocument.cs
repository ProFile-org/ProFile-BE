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

namespace Application.Borrows.Commands;

public class CheckoutDocument
{
    public record Command : IRequest<BorrowDto>
    {
        public User CurrentStaff { get; init; } = null!;
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
                .ThenInclude(x => x.Folder!)
                .ThenInclude(x => x.Locker)
                .ThenInclude(x => x.Room)
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

            var staff = await _context.Staffs
                .Include(x => x.Room)
                .FirstOrDefaultAsync(x => x.Id == request.CurrentStaff.Id, cancellationToken);

            if (staff is null)
            {
                throw new KeyNotFoundException("Staff does not exist.");
            }

            if (staff.Room is null)
            {
                throw new ConflictException("Staff does not have a room.");
            }

            if (staff.Room.Id != borrowRequest.Document.Folder!.Locker.Room.Id)
            {
                throw new ConflictException("Request cannot be checked out due to different room.");
            }
            
            var localDateTimeNow = LocalDateTime.FromDateTime(_dateTimeProvider.DateTimeNow);
            borrowRequest.Status = BorrowRequestStatus.CheckedOut;
            borrowRequest.Document.Status = DocumentStatus.Borrowed;
            borrowRequest.Document.LastModified = localDateTimeNow;
            borrowRequest.Document.LastModifiedBy = request.CurrentStaff.Id;
            var log = new DocumentLog()
            {
                ObjectId = borrowRequest.Document.Id,
                UserId =  request.CurrentStaff.Id,
                User =  request.CurrentStaff,
                Time = localDateTimeNow,
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