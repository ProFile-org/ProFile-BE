using Application.Common.Models.Operations;
using Domain.Entities;
using Domain.Entities.Physical;

namespace Application.Common.Interfaces;

public interface IPermissionManager
{
    bool IsGranted(Guid documentId, DocumentOperation operation, params Guid[] userIds);
    Task GrantAsync(Document document, DocumentOperation operation, User[] users, DateTime expiryDate, CancellationToken cancellationToken);
    Task RevokeAsync(Guid documentId, DocumentOperation operation, Guid[] userIds, CancellationToken cancellationToken);
}