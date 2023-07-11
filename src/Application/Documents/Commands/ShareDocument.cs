using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Logging;
using Application.Common.Models.Dtos.Physical;
using Application.Common.Models.Operations;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Physical;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<ShareDocument> _logger;

        public CommandHandler(IApplicationDbContext applicationDbContext, IMapper mapper, IPermissionManager permissionManager, IDateTimeProvider dateTimeProvider, ILogger<ShareDocument> logger)
        {
            _applicationDbContext = applicationDbContext;
            _mapper = mapper;
            _permissionManager = permissionManager;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
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

            await HandlePermissionGrantOrRevoke(request.CanRead, document, DocumentOperation.Read, user, request.ExpiryDate.ToLocalTime(), request.CurrentUser.Id, cancellationToken);
            await HandlePermissionGrantOrRevoke(request.CanBorrow, document, DocumentOperation.Borrow, user, request.ExpiryDate.ToLocalTime(), request.CurrentUser.Id, cancellationToken);

            await _applicationDbContext.SaveChangesAsync(cancellationToken);
            return _mapper.Map<DocumentDto>(document);
        }

        private async Task HandlePermissionGrantOrRevoke(
            bool canPerformAction,
            Document document,
            DocumentOperation operation,
            User user,
            DateTime expiryDate,
            Guid currentUserId,
            CancellationToken cancellationToken)
        {
            var isGranted = _permissionManager.IsGranted(document.Id, operation, user.Id);

            if (canPerformAction && !isGranted)
            {
                await GrantPermission(document, operation, user, expiryDate, currentUserId, cancellationToken);
            }

            if (!canPerformAction && isGranted)
            {
                await RevokePermission(document, operation, user, currentUserId, cancellationToken);
            }
        }

        private async Task GrantPermission(
            Document document,
            DocumentOperation operation,
            User user,
            DateTime expiryDate,
            Guid currentUserId,
            CancellationToken cancellationToken)
        {
            await _permissionManager.GrantAsync(document, operation, new[] { user }, expiryDate, cancellationToken);

            // log
            using (Logging.PushProperties(nameof(Document), document.Id, currentUserId))
            {
                switch(operation)
                {
                    case DocumentOperation.Read:
                        _logger.LogGrantPermission(DocumentOperation.Read.ToString(), user.Username);
                        break;
                    case DocumentOperation.Borrow:
                        _logger.LogGrantPermission(DocumentOperation.Borrow.ToString(), user.Username);
                        break;
                }
            }
        }

        private async Task RevokePermission(
            Document document,
            DocumentOperation operation,
            User user,
            Guid currentUserId,
            CancellationToken cancellationToken)
        {
            await _permissionManager.RevokeAsync(document.Id, operation, new[] { user.Id }, cancellationToken);

            // log
            using (Logging.PushProperties(nameof(Document), document.Id, currentUserId))
            {
                switch(operation)
                {
                    case DocumentOperation.Read:
                        _logger.LogRevokePermission(DocumentOperation.Read.ToString(), user.Username);
                        break;
                    case DocumentOperation.Borrow:
                        _logger.LogRevokePermission(DocumentOperation.Borrow.ToString(), user.Username);
                        break;
                }
            }
        }
    }
}