using Application.Common.Models.Dtos.Physical;
using MediatR;
using Application.Common.Interfaces;
using Application.Common.Exceptions;
using AutoMapper;
using Domain.Entities.Physical;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Application.Documents.Commands;

public class BorrowDocument
{
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Reason)
                .MaximumLength(512).WithMessage("Reason cannot exceed 512 characters.");
        }
    }
    
    public record Command : IRequest<BorrowDto>
    {
        public Guid DocumentId { get; set; }
        public Guid BorrowerId { get; set; }
        public DateTime BorrowTo { get; set; }
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
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == request.BorrowerId, cancellationToken);
            if (user is null)
            {
                throw new KeyNotFoundException("User does not exist.");
            }
            
            var document = await _context.Documents.FirstOrDefaultAsync(x => x.Id == request.DocumentId, cancellationToken);
            if (document is null)
            {
                throw new KeyNotFoundException("Document does not exist.");
            }
            
            if (DateTime.Now > request.BorrowTo)
            {
                throw new ConflictException("Due date cannot be in the past.");
            }

            var entity = new Borrow()
            {
                Borrower = user,
                Document = document,
                BorrowTime = LocalDateTime.FromDateTime(DateTime.Now),
                DueTime = LocalDateTime.FromDateTime(request.BorrowTo),
                Reason = request.Reason,
            };

            var result = await _context.Borrows.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return _mapper.Map<BorrowDto>(result.Entity);
        }
    }
}