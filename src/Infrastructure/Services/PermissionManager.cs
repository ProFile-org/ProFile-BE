using System.Configuration;
using Application.Common.Interfaces;
using Application.Common.Models.Operations;
using Domain.Entities;
using Domain.Entities.Physical;
using NodaTime;

namespace Infrastructure.Services;

public class PermissionManager : IPermissionManager
{
    private readonly IApplicationDbContext _context;

    public PermissionManager(IApplicationDbContext context)
    {
        _context = context;
    }

    public bool IsGranted(Guid documentId, DocumentOperation operation, params Guid[] userIds)
    {
        return Array.TrueForAll(userIds, id => !_context.Permissions.Any(x =>
            x.DocumentId == documentId && x.EmployeeId == id &&
            !x.AllowedOperations.Contains(operation.ToString())));
    }

    public async Task GrantAsync(Document document, DocumentOperation operation, User[] users, DateTime expiryDate, CancellationToken cancellationToken)
    {
        foreach (var user in users)
        {
            var existedPermission =
                _context.Permissions.FirstOrDefault(x => x.DocumentId == document.Id && x.EmployeeId == user.Id);

            if (existedPermission is not null)
            {
                var operations = existedPermission.AllowedOperations.Split(",");
                if (operations.Contains(operation.ToString()))
                {
                    continue;
                }

                var x = new CommaDelimitedStringCollection
                {
                    existedPermission.AllowedOperations,
                    operation.ToString()
                };
                existedPermission.AllowedOperations = x.ToString();
                existedPermission.ExpiryDateTime = LocalDateTime.FromDateTime(expiryDate);
                _context.Permissions.Update(existedPermission);
            }
            else
            {
                existedPermission = new Permission()
                {
                    DocumentId = document.Id,
                    EmployeeId = user.Id,
                    Document = document,
                    Employee = user,
                    AllowedOperations = operation.ToString(),
                };
                existedPermission.ExpiryDateTime = LocalDateTime.FromDateTime(expiryDate);
                await _context.Permissions.AddAsync(existedPermission, cancellationToken);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokeAsync(Guid documentId, DocumentOperation operation, Guid[] userIds, CancellationToken cancellationToken)
    {
        foreach (var userId in userIds)
        {
            var existedPermission =
                _context.Permissions.FirstOrDefault(x => x.DocumentId == documentId && x.EmployeeId == userId);
            if (existedPermission is null) continue;
            var operations = existedPermission.AllowedOperations.Split(",");
            if (!operations.Contains(operation.ToString())) continue;

            var x = new CommaDelimitedStringCollection();
            x.AddRange(operations);
            x.Remove(operation.ToString());
            if (x.Count == 0 || existedPermission.ExpiryDateTime < LocalDateTime.FromDateTime(DateTime.Now))
            {
                _context.Permissions.Remove(existedPermission);
            }
            else
            {
                existedPermission.AllowedOperations = x.ToString();
                _context.Permissions.Update(existedPermission);
            }
        }
        await _context.SaveChangesAsync(cancellationToken);
    }
}