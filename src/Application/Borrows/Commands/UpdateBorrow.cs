using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using Domain.Entities.Physical;
using Domain.Statuses;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Application.Borrows.Commands;

public class UpdateBorrow
{
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;
            
            RuleFor(x => x.Reason)
                .MaximumLength(512).WithMessage("Reason cannot exceed 512 characters.");

            RuleFor(x => x.BorrowFrom)
                .GreaterThan(DateTime.Now).WithMessage("Borrow date cannot be in the past.")
                .Must((command, borrowTime) => borrowTime < command.BorrowTo).WithMessage("Due date cannot be before borrow date.");

            RuleFor(x => x.BorrowTo)
                .GreaterThan(DateTime.Now).WithMessage("Due date cannot be in the past.");
        }
    }

    public record Command : IRequest<BorrowDto>
    {
        public Guid PerformingUserId { get; init; }
        public Guid BorrowId { get; init; }
        public DateTime BorrowFrom { get; init; }
        public DateTime BorrowTo { get; init; }
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
                throw new ConflictException("Cannot update borrow request.");
            }

            if (borrowRequest.Document.Status is DocumentStatus.Lost)
            {
                throw new ConflictException("Document is lost.");
            }

            var localDateTimeNow = LocalDateTime.FromDateTime(DateTime.Now);
            var existedBorrows = _context.Borrows
                .Include(x => x.Borrower)
                .Where(x =>
                    x.Document.Id == borrowRequest.Document.Id
                    && x.Id != borrowRequest.Id
                    && ((x.DueTime > localDateTimeNow)
                        || x.Status == BorrowRequestStatus.Overdue));

            var borrowFromTime = LocalDateTime.FromDateTime(request.BorrowFrom);
            var borrowToTime = LocalDateTime.FromDateTime(request.BorrowTo);
            foreach (var borrow in existedBorrows)
            {
                if (borrow.Status 
                        is BorrowRequestStatus.Approved 
                        or BorrowRequestStatus.CheckedOut
                    && (borrowFromTime <= borrow.DueTime && borrowToTime >= borrow.BorrowTime))
                {
                    throw new ConflictException("This document cannot be updated.");
                }
            }
            borrowRequest.BorrowTime = borrowFromTime;
            borrowRequest.DueTime = borrowToTime;
            borrowRequest.Reason = request.Reason;

            var result = _context.Borrows.Update(borrowRequest);
            await _context.SaveChangesAsync(cancellationToken);

            return _mapper.Map<BorrowDto>(result.Entity);
        }
    }
}