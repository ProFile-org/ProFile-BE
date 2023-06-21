using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Messages;
using Application.Common.Models.Dtos.Physical;
using Application.Common.Models.Operations;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Logging;
using Domain.Entities.Physical;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Application.Documents.Commands;

public class ShareDocument
{
    public record Command : IRequest<DocumentDto>
    {
        public User CurrentUser { get; init; } = null!;
        public Guid DocumentId { get; init; }
        public Guid UserId { get; init; }
        public bool CanRead { get; init; }
        public bool CanBorrow { get; init; }
        public DateTime ExpiryDate { get; init; }
    }
    
    public class CommandHandler : IRequestHandler<Command, DocumentDto>
    {
        private readonly IApplicationDbContext _applicationDbContext;
        private readonly IMapper _mapper;
        private readonly IPermissionManager _permissionManager;
        private readonly IDateTimeProvider _dateTimeProvider;

        public CommandHandler(IApplicationDbContext applicationDbContext, IMapper mapper, IPermissionManager permissionManager, IDateTimeProvider dateTimeProvider)
        {
            _applicationDbContext = applicationDbContext;
            _mapper = mapper;
            _permissionManager = permissionManager;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<DocumentDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var document = await
                _applicationDbContext.Documents
                    .Include(x => x.Importer)
                    .FirstOrDefaultAsync(x => x.Id == request.DocumentId, cancellationToken);
            if (document is null)
            {
                throw new KeyNotFoundException("Document does not exist.");
            }

            if (document.Importer!.Id != request.CurrentUser.Id)
            {
                throw new UnauthorizedAccessException("You are not the owner of the document.");
            }

            var user = await _applicationDbContext.Users.FirstOrDefaultAsync(x => x.Id == request.UserId, cancellationToken);

            if (user is null)
            {
                throw new KeyNotFoundException("User does not exist.");
            }

            if (request.ExpiryDate.ToUniversalTime() < DateTime.UtcNow)
            {
                throw new ConflictException("Expiry date cannot be in the past.");
            }
            
            var localDateTimeNow = LocalDateTime.FromDateTime(_dateTimeProvider.DateTimeNow);

            var log = new DocumentLog()
            {
                ObjectId = document.Id,
                Time = localDateTimeNow,
                User = request.CurrentUser,
                UserId = request.CurrentUser.Id,
                Action = string.Empty,
            };

            await HandlePermissionGrantOrRevoke(request.CanRead, document, DocumentOperation.Read, user, request.ExpiryDate.ToLocalTime(), cancellationToken, log);
            await HandlePermissionGrantOrRevoke(request.CanBorrow, document, DocumentOperation.Borrow, user, request.ExpiryDate.ToLocalTime(), cancellationToken, log);

            if (!string.IsNullOrEmpty(log.Action))
            {
                await _applicationDbContext.DocumentLogs.AddAsync(log, cancellationToken);
            }
            await _applicationDbContext.SaveChangesAsync(cancellationToken);
            return _mapper.Map<DocumentDto>(document);
        }

        private async Task HandlePermissionGrantOrRevoke(
            bool canPerformAction,
            Document document,
            DocumentOperation operation,
            User user,
            DateTime expiryDate,
            CancellationToken cancellationToken,
            DocumentLog log)
        {
            var isGranted = _permissionManager.IsGranted(document.Id, operation, user.Id);

            if (canPerformAction && !isGranted)
            {
                await GrantPermission(document, operation, user, expiryDate, log, cancellationToken);
            }

            if (!canPerformAction && isGranted)
            {
                await RevokePermission(document, operation, user, log, cancellationToken);
            }
        }

        private async Task GrantPermission(
            Document document,
            DocumentOperation operation,
            User user,
            DateTime expiryDate,
            DocumentLog log,
            CancellationToken cancellationToken)
        {
            await _permissionManager.GrantAsync(document, operation, new[] { user }, expiryDate, cancellationToken);

            // log
            log.Action = operation switch
            {
                DocumentOperation.Read => DocumentLogMessages.GrantRead(user.Username),
                DocumentOperation.Borrow => DocumentLogMessages.GrantBorrow(user.Username),
                _ => log.Action
            };
        }

        private async Task RevokePermission(
            Document document,
            DocumentOperation operation,
            User user,
            DocumentLog log,
            CancellationToken cancellationToken)
        {
            await _permissionManager.RevokeAsync(document.Id, operation, new[] { user.Id }, cancellationToken);

            // log
            log.Action = operation switch
            {
                DocumentOperation.Read => DocumentLogMessages.RevokeRead(user.Username),
                DocumentOperation.Borrow => DocumentLogMessages.RevokeBorrow(user.Username),
                _ => log.Action
            };
        }
    }
}