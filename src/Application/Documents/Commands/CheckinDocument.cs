using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Messages;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using Domain.Entities.Logging;
using Domain.Entities.Physical;
using Domain.Statuses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Application.Documents.Commands;

public class CheckinDocument
{
    public record Command : IRequest<DocumentDto>
    {
        public Guid PerformingUserId { get; init; }
        public Guid DocumentId { get; init; }
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
            var document = await
                _context.Documents.FirstOrDefaultAsync(x => x.Id == request.DocumentId, cancellationToken);

            if (document is null)
            {
                throw new KeyNotFoundException("Document does not exist.");
            }

            var performingUser = await _context.Users.FirstOrDefaultAsync(x => x.Id == request.PerformingUserId, cancellationToken);
            document.Status = DocumentStatus.Available;
            document.LastModified = LocalDateTime.FromDateTime(DateTime.Now);
            document.LastModifiedBy = performingUser!.Id;
            var log = new DocumentLog()
            {
                User = performingUser,
                UserId = performingUser.Id,
                Object = document,
                Time = LocalDateTime.FromDateTime(DateTime.Now),
                Action = DocumentLogMessages.Import.Checkin,
            };

            var result = _context.Documents.Update(document);
            await _context.DocumentLogs.AddAsync(log, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<DocumentDto>(result.Entity);
        }
    }
}