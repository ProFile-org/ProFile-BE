using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Operations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Documents.Commands;

public class ShareDocument
{
    public record Command : IRequest<bool>
    {
        public Guid PerformingUserId { get; init; }
        public Guid DocumentId { get; init; }
        public Guid[] UserIds { get; init; } = null!;
        public bool CanRead { get; init; }
        public bool CanBorrow { get; init; }
        public DateTime ExpiryDate { get; init; }
    }
    
    public class CommandHandler : IRequestHandler<Command, bool>
    {
        private readonly IApplicationDbContext _applicationDbContext;
        private readonly IPermissionManager _permissionManager;

        public CommandHandler(IApplicationDbContext applicationDbContext, IPermissionManager permissionManager)
        {
            _permissionManager = permissionManager;
            _applicationDbContext = applicationDbContext;
        }

        public async Task<bool> Handle(Command request, CancellationToken cancellationToken)
        {
            var document = await
                _applicationDbContext.Documents
                    .Include(x => x.Importer)
                    .FirstOrDefaultAsync(x => x.Id == request.DocumentId, cancellationToken);
            if (document is null)
            {
                throw new KeyNotFoundException("Document does not exist.");
            }

            if (document.Importer!.Id != request.PerformingUserId)
            {
                throw new UnauthorizedAccessException("You are not the owner of the document.");
            }

            if (request.ExpiryDate.ToUniversalTime() < DateTime.UtcNow)
            {
                throw new ConflictException("Expiry date cannot be in the past.");
            }
            
            var users = _applicationDbContext.Users
                .Where(x => request.UserIds.Contains(x.Id))
                .ToList();
            users.RemoveAll(x => x.Id == request.PerformingUserId);

            if (request.CanRead)
            {
                await _permissionManager.GrantAsync(document, DocumentOperation.Read, users.ToArray(), request.ExpiryDate.ToLocalTime(), cancellationToken);
            }
            else
            {
                await _permissionManager.RevokeAsync(document.Id, DocumentOperation.Read, request.UserIds, cancellationToken);
            }

            if (request.CanBorrow)
            {
                await _permissionManager.GrantAsync(document, DocumentOperation.Borrow, users.ToArray(), request.ExpiryDate.ToLocalTime(), cancellationToken);
            }
            else
            {
                await _permissionManager.RevokeAsync(document.Id, DocumentOperation.Borrow, request.UserIds, cancellationToken);
            }

            return true;
        }
    }
}