using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Messages;
using Application.Common.Models.Dtos.ImportDocument;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Logging;
using Domain.Statuses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Application.Documents.Commands;

public class ApproveDocument
{
    public record Command : IRequest<DocumentDto>
    {
        public User CurrentUser { get; init; } = null!;
        public Guid DocumentId { get; init; }
        public string Decision { get; init; } = null!;
        public string Reason { get; init; } = null!;
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
                throw new ConflictException("Request cannot be approved or rejected.");
            }

            if (IsApproval(request.Decision))
            {
                document.Status = DocumentStatus.Approved;
            }
            
            if (IsRejection(request.Decision))
            {
                document.Status = DocumentStatus.Rejected;
            }
            
            var log = new DocumentLog()
            {
                Object = document,
                Time = LocalDateTime.FromDateTime(DateTime.Now),
                User = request.CurrentUser,
                UserId = request.CurrentUser.Id,
                Action = DocumentLogMessages.Import.Approve,
            };
            var requestLog = new RequestLog()
            {
                Object = document,
                Time = LocalDateTime.FromDateTime(DateTime.Now),
                User = request.CurrentUser,
                UserId = request.CurrentUser.Id,
                Action = RequestLogMessages.ApproveImport,
                Reason = request.Reason,
            };
            var result = _context.Documents.Update(document);
            await _context.DocumentLogs.AddAsync(log, cancellationToken);
            await _context.RequestLogs.AddAsync(requestLog, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<DocumentDto>(result.Entity);
        }

        private static bool IsApproval(string decision)
            => decision.ToLower().Trim().Equals("approve");
        
        private static bool IsRejection(string decision)
            => decision.ToLower().Trim().Equals("reject");
    }
}