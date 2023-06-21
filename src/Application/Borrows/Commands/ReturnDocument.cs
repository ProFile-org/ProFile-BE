using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using Domain.Statuses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Application.Borrows.Commands;

public class ReturnDocument
{
    public record Command : IRequest<BorrowDto>
    {
        public Guid CurrentUserId { get; init; }
        public Guid DocumentId { get; init; }
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
                .ThenInclude(x => x.Folder!)
                .ThenInclude(x => x.Locker)
                .ThenInclude(x => x.Room)
                .FirstOrDefaultAsync(x => x.Document.Id == request.DocumentId
                    && x.Status == BorrowRequestStatus.CheckedOut, cancellationToken);
            if (borrowRequest is null)
            {
                throw new KeyNotFoundException("Borrow request does not exist.");
            }

            if (borrowRequest.Document.Status is not DocumentStatus.Borrowed)
            {
                throw new ConflictException("Document is not borrowed.");
            }

            if (borrowRequest.Status is not BorrowRequestStatus.CheckedOut)
            {
                throw new ConflictException("Request cannot be made.");
            }
            
            var staff = await _context.Staffs
                .Include(x => x.Room)
                .FirstOrDefaultAsync(x => x.Id == request.CurrentUserId, cancellationToken);

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

            borrowRequest.Status = BorrowRequestStatus.Returned;
            borrowRequest.Document.Status = DocumentStatus.Available;
            borrowRequest.ActualReturnTime = LocalDateTime.FromDateTime(DateTime.Now);
            var result = _context.Borrows.Update(borrowRequest);
            _context.Documents.Update(borrowRequest.Document);
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<BorrowDto>(result.Entity);
        }
    }
}