using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Logging;
using Application.Common.Messages;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using Domain.Entities;
using Domain.Statuses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Application.Borrows.Commands;

public class ReturnDocument
{
    public record Command : IRequest<BorrowDto>
    {
        public User CurrentUser { get; init; } = null!;
        public Guid DocumentId { get; init; }
    }

    public class CommandHandler : IRequestHandler<Command, BorrowDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger<ReturnDocument> _logger;

        public CommandHandler(IApplicationDbContext context, IMapper mapper, IDateTimeProvider dateTimeProvider, ILogger<ReturnDocument> logger)
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
                .FirstOrDefaultAsync(x => x.Id == request.CurrentUser.Id, cancellationToken);

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
            
            borrowRequest.Status = BorrowRequestStatus.Returned;
            borrowRequest.Document.Status = DocumentStatus.Available;
            borrowRequest.ActualReturnTime = localDateTimeNow;
            
            var result = _context.Borrows.Update(borrowRequest);
            _context.Documents.Update(borrowRequest.Document);
            await _context.SaveChangesAsync(cancellationToken);
            using (Logging.PushProperties("BorrowRequest", borrowRequest.Id, request.CurrentUser.Id))
            {
                Common.Extensions.Logging.BorrowLogExtensions.LogReturnDocument(_logger, borrowRequest.Document.Id.ToString(), borrowRequest.Id.ToString());
            }
            return _mapper.Map<BorrowDto>(result.Entity);
        }
    }
}