using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using Domain.Entities;
using Domain.Statuses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Application.Borrows.Commands;

public class ReportFoundDocument
{
    public record Command : IRequest<BorrowDto>
    {
        public User CurrentUser { get; init; } = null!;
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

            var staff = await _context.Staffs
                .Include(x => x.Room)
                .FirstOrDefaultAsync(x => x.Id == request.CurrentUser.Id, cancellationToken);
            
            if (staff is null)
            {
                throw new KeyNotFoundException("Staff does not exist.");
            }
            
            if (staff.Room is null)
            {
                throw new ConflictException("Staff does not have a room.");
            }

            // Staff cant report lost documents from other department 
            if (staff.Room.Id != borrowRequest.Document.Folder!.Locker.Room.Id)
            {
                throw new ConflictException("Request cannot be checked out due to different room.");
            }

            if (borrowRequest.Document.Status is not DocumentStatus.Lost)
            {
                throw new ConflictException("Document is not lost.");
            }
            
            if (borrowRequest.Status is not BorrowRequestStatus.Lost)
            {
                throw new ConflictException("Request is not lost.");
            }
            
            // Get borrows for the lost document which are not processable 
            var borrowsForDocument = _context.Borrows
                .Include(x => x.Document)
                .Where( x => x.Id != request.BorrowId 
                             && x.Document.Id == borrowRequest.Document.Id 
                             && x.Status == BorrowRequestStatus.NotProcessable);
            
            var localDateTimeNow = LocalDateTime.FromDateTime(_dateTimeProvider.DateTimeNow);
            
            foreach (var borrow in borrowsForDocument)
            {
                if (borrow.DueTime < localDateTimeNow)
                {
                    borrow.Status = BorrowRequestStatus.Pending;
                }
            }

            borrowRequest.Status = BorrowRequestStatus.Returned;
            borrowRequest.Document.Status = DocumentStatus.Available;
            borrowRequest.ActualReturnTime = localDateTimeNow;
            
            var result = _context.Borrows.Update(borrowRequest);
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<BorrowDto>(result.Entity);
        }
    }
}