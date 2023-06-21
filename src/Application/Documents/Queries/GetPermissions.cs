using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Physical;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Documents.Queries;

public class GetPermissions
{
    public record Query : IRequest<PermissionDto>
    {
        public User CurrentUser { get; init; } = null!;
        public Guid DocumentId { get; init; }
    }

    public class QueryHandler : IRequestHandler<Query, PermissionDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public QueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PermissionDto> Handle(Query request, CancellationToken cancellationToken)
        {
            var document = await GetDocumentWithImporter(request.DocumentId, cancellationToken);
            if (document is null)
            {
                throw new ConflictException("Document does not exist.");
            }

            if (IsOwner(request.CurrentUser.Id, document))
            {
                return CreatePermissionDto(document.Id, request.CurrentUser.Id, true, true);
            }

            var permission = await GetPermission(request.DocumentId, request.CurrentUser.Id, cancellationToken);

            if (permission is null)
            {
                return CreatePermissionDto(document.Id, request.CurrentUser.Id, false, false);
            }

            return _mapper.Map<PermissionDto>(permission);
        }

        private async Task<Document?> GetDocumentWithImporter(Guid documentId, CancellationToken cancellationToken)
        {
            return await _context.Documents
                .Include(x => x.Importer)
                .FirstOrDefaultAsync(x => x.Id == documentId, cancellationToken);
        }

        private static bool IsOwner(Guid userId, Document document)
        {
            return document.Importer!.Id == userId;
        }

        private async Task<Permission?> GetPermission(Guid documentId, Guid employeeId, CancellationToken cancellationToken)
        {
            return await _context.Permissions.FirstOrDefaultAsync(
                x => x!.DocumentId == documentId && x.EmployeeId == employeeId,
                cancellationToken);
        }

        private static PermissionDto CreatePermissionDto(Guid documentId, Guid employeeId, bool canRead, bool canBorrow)
        {
            return new PermissionDto()
            {
                DocumentId = documentId,
                EmployeeId = employeeId,
                CanRead = canRead,
                CanBorrow = canBorrow,
            };
        }
    }
}