using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Physical;
using Application.Common.Models.Operations;
using Application.Identity;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Documents.Queries;

public class GetDocumentById
{
    public record Query : IRequest<DocumentDto>
    {
        public Guid DocumentId { get; init; } 
    }

    public class QueryHandler : IRequestHandler<Query, DocumentDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly IPermissionManager _permissionManager;

        public QueryHandler(IApplicationDbContext context, IMapper mapper, ICurrentUserService currentUserService, IPermissionManager permissionManager)
        {
            _context = context;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _permissionManager = permissionManager;
        }
        public async Task<DocumentDto> Handle(Query request, CancellationToken cancellationToken)
        {
            var document = await _context.Documents
                .Include(x => x.Department)
                .Include(x => x.Importer)
                .FirstOrDefaultAsync(x => x.Id == request.DocumentId, cancellationToken);
            
            if (document is null)
            {
                throw new KeyNotFoundException("Document does not exist.");
            }
            
            var performingUser = _currentUserService.GetCurrentUser();
            
            if (performingUser.Role.Equals(IdentityData.Roles.Admin))
            {
                return _mapper.Map<DocumentDto>(document);
            }

            if (performingUser.Role.Equals(IdentityData.Roles.Staff))
            {
                var departmentIdOfStaff = _currentUserService.GetCurrentDepartmentForStaff();

                if (departmentIdOfStaff!.Value != document.Department!.Id)
                {
                    throw new ConflictException("You don't have permission to view this document.");
                }
                return _mapper.Map<DocumentDto>(document);
            }
        
            var isGranted = _permissionManager.IsGranted(document.Id, DocumentOperation.Read, performingUser.Id);
            if (!isGranted)
            {
                throw new UnauthorizedAccessException("You don't have permission to view this document.");
            }
            
            return _mapper.Map<DocumentDto>(document);
        }
    }
}