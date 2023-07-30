using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Logging;
using Application.Common.Models.Dtos.Physical;
using Application.Common.Models.Operations;
using AutoMapper;
using Domain.Entities.Physical;
using Domain.Events;
using Domain.Statuses;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Application.Borrows.Commands;

public class BorrowDocument
{
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.BorrowReason)
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
        public Guid DocumentId { get; init; }
        public Guid BorrowerId { get; init; }
        public DateTime BorrowFrom { get; init; }
        public DateTime BorrowTo { get; init; }
        public string BorrowReason { get; init; } = null!;
    }

    public class CommandHandler : IRequestHandler<Command, BorrowDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IPermissionManager _permissionManager;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger<BorrowDocument> _logger;

        public CommandHandler(IApplicationDbContext context, IMapper mapper, IPermissionManager permissionManager, IDateTimeProvider dateTimeProvider, ILogger<BorrowDocument> logger)
        {
            _context = context;
            _mapper = mapper;
            _permissionManager = permissionManager;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
        }

        public async Task<BorrowDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .Include(x => x.Department)
                .FirstOrDefaultAsync(x => x.Id == request.BorrowerId, cancellationToken);
            if (user is null)
            {
                throw new KeyNotFoundException("User does not exist.");
            }
            
            if (user.IsActive is false)
            {
                throw new ConflictException("User is not active.");
            }
            
            if (user.IsActivated is false)
            {
                throw new ConflictException("User is not activated.");
            }
            
            var document = await _context.Documents
                .Include(x => x.Department)
                .Include(x => x.Importer)
                .FirstOrDefaultAsync(x => x.Id == request.DocumentId, cancellationToken);
            if (document is null)
            {
                throw new KeyNotFoundException("Document does not exist.");
            }

            if (document.Status is not DocumentStatus.Available)
            {
                throw new ConflictException("Document is not available.");
            }
            
            if (document.Department!.Id != user.Department!.Id)
            {
                throw new ConflictException("User is not allowed to borrow this document.");
            }

            // getting out a request of that document which is either not due or overdue
            // if the request is in time, meaning not overdue,
            // then check if its due date is less than the borrow request date, if not then check
            // if it's already been approved, checked out or lost, meaning 
            var localDateTimeNow = LocalDateTime.FromDateTime(_dateTimeProvider.DateTimeNow);
            var borrowFromTime = LocalDateTime.FromDateTime(request.BorrowFrom);
            var borrowToTime = LocalDateTime.FromDateTime(request.BorrowTo);
            var existedBorrows =  _context.Borrows
                .Include(x => x.Borrower)
                .Where(x =>
                    x.Document.Id == request.DocumentId
                    && (x.DueTime > localDateTimeNow
                        || x.Status == BorrowRequestStatus.Overdue));

            foreach (var borrow in existedBorrows)
            {
                // Does not make sense if the same person go up and want to borrow the same document again
                // even if the borrow day will be after the due day
                if (borrow.Borrower.Id == request.BorrowerId
                && borrow.Status is BorrowRequestStatus.Pending
                    or BorrowRequestStatus.Approved)
                {
                    throw new ConflictException("This document is already requested borrow from the same user.");
                }

                if ((borrow.Status
                    is BorrowRequestStatus.Approved
                    or BorrowRequestStatus.CheckedOut)
                    && (borrowFromTime <= borrow.DueTime && borrowToTime >= borrow.BorrowTime))
                {
                    throw new ConflictException("This document cannot be borrowed.");
                }
            }

            var entity = new Borrow()
            {
                Borrower = user,
                Document = document,
                BorrowTime = borrowFromTime,
                DueTime = borrowToTime,
                BorrowReason = request.BorrowReason,
                StaffReason = string.Empty,
                Status = BorrowRequestStatus.Pending,
                Created = localDateTimeNow,
                CreatedBy = user.Id,
            };
            
            if (document.IsPrivate)
            {
                var isGranted = _permissionManager.IsGranted(request.DocumentId, DocumentOperation.Borrow, request.BorrowerId);
                if (document.ImporterId != request.BorrowerId && !isGranted)
                {
                    throw new UnauthorizedAccessException("You don't have permission to borrow this document.");
                }
                entity.Status = BorrowRequestStatus.Approved;
            }

            
            var result = await _context.Borrows.AddAsync(entity, cancellationToken);
            entity.AddDomainEvent(new RequestCreated($"{user.FirstName} {user.LastName}", "borrow request", "borrow",
                document.Title, entity.Id, request.BorrowReason, document.Id));
            await _context.SaveChangesAsync(cancellationToken);
            using (Logging.PushProperties("Request", document.Id, user.Id))
            {
                Common.Extensions.Logging.BorrowLogExtensions.LogBorrowDocument(_logger, document.Id.ToString(), result.Entity.Id.ToString());
            }
            return _mapper.Map<BorrowDto>(result.Entity);
        }
    }
}