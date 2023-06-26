using Application.Common.Exceptions;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Physical;
using Application.Common.Models.Operations;
using Application.Identity;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Physical;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Documents.Queries;

public class GetDocumentById
{
    public record Query : IRequest<DocumentDto>
    {
        public User CurrentUser { get; init; } = null!;
        public Guid DocumentId { get; init; }
    }

    public class QueryHandler : IRequestHandler<Query, DocumentDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IPermissionManager _permissionManager;

        public QueryHandler(
            IApplicationDbContext context,
            IMapper mapper,
            IPermissionManager permissionManager)
        {
            _context = context;
            _mapper = mapper;
            _permissionManager = permissionManager;
        }

        public async Task<DocumentDto> Handle(Query request, CancellationToken cancellationToken)
        {
            var document = await _context.Documents
                .Include(x => x.Folder)
                .ThenInclude(x => x.Locker)
                .ThenInclude(x => x.Room)
                .Include(x => x.Department)
                .Include(x => x.Importer)
                .FirstOrDefaultAsync(x => x.Id == request.DocumentId, cancellationToken);
            
            if (document is null)
            {
                throw new KeyNotFoundException("Document does not exist.");
            }
            
            if (ViolateConstraints(request.CurrentUser, document))
            {
                throw new UnauthorizedAccessException("You don't have permission to view this document.");
            }
            
            return _mapper.Map<DocumentDto>(document);
        }

        private bool ViolateConstraints(User user, Document document)
            => IsStaffAndNotInSameDepartment(user, document)
               || IsEmployeeAndDoesNotHasReadPermission(user, document);

        private static bool IsStaffAndNotInSameDepartment(User user, Document document)
            => user.Role.IsStaff()
               && user.Department!.Id != document.Department!.Id;

        private bool IsEmployeeAndDoesNotHasReadPermission(User user, Document document)
            => user.Role.IsEmployee()
               && document.ImporterId != user.Id
               && !_permissionManager.IsGranted(document.Id, DocumentOperation.Read, user.Id);
    }
}