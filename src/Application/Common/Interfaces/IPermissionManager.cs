using Application.Common.Models.Operations;
using Domain.Entities;
using Domain.Entities.Physical;

namespace Application.Common.Interfaces;

public interface IPermissionManager
{
    bool IsGranted(Guid documentId, DocumentOperation operation, params Guid[] userIds);
    Task GrantAsync(Document document, DocumentOperation operation, User[] users, CancellationToken cancellationToken);
    Task RevokeAsync(Document document, DocumentOperation operation, User[] users, CancellationToken cancellationToken);
}