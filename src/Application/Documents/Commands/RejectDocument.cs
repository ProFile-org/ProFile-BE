using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Messages;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using Domain.Entities.Logging;
using Domain.Statuses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Application.Documents.Commands;

public class RejectDocument
{
    public record Command : IRequest<DocumentDto>
    {
        public Guid PerformingUserId { get; set; }
        public Guid DocumentId { get; set; }
    }

    public class CommandHandler : IRequestHandler<Command, DocumentDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CommandHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<DocumentDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var document = await _context.Documents
                .Include(x => x.Department)
                .FirstOrDefaultAsync(x =>
                x.Id == request.DocumentId, cancellationToken);
            if (document is null)
            {
                throw new ConflictException("Document does not exist.");
            }

            if (document.Status is not DocumentStatus.Issued)
            {
                throw new ConflictException("Request cannot be rejected.");
            }

            document.Status = DocumentStatus.Rejected;
            var performingUser = await _context.Users.FirstOrDefaultAsync(x => x.Id == request.PerformingUserId, cancellationToken);
            var log = new DocumentLog()
            {
                Object = document,
                Time = LocalDateTime.FromDateTime(DateTime.Now),
                User = performingUser!,
                UserId = performingUser!.Id,
                Action = DocumentLogMessages.Import.Reject,
            };
            var result = _context.Documents.Update(document);
            await _context.DocumentLogs.AddAsync(log, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<DocumentDto>(result.Entity);
        }
    }
}