using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using Domain.Statuses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Borrows.Commands;

public class ReportLostDocument
{
    public record Command : IRequest<BorrowDto>
    {
        public Guid BorrowId { get; init; }
    }
    
    public class CommandHander : IRequestHandler<Command, BorrowDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CommandHander(IApplicationDbContext context, IMapper mapper)
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

            if (borrowRequest.Document.Status is not DocumentStatus.Borrowed)
            {
                throw new ConflictException("Document is not borrowed.");
            }
            
            if (borrowRequest.Status is not (BorrowRequestStatus.Overdue or BorrowRequestStatus.CheckedOut))
            {
                throw new ConflictException("Request cannot be lost.");
            }

            borrowRequest.Status = BorrowRequestStatus.Lost;
            borrowRequest.Document.Status = DocumentStatus.Lost;
            var result = _context.Borrows.Update(borrowRequest);
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<BorrowDto>(result.Entity);
        }
    }
}