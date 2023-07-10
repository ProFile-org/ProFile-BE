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

            if (ViolateConstraintForStaffAndEmployee(request.CurrentUser, document)) {
                throw new UnauthorizedAccessException("You don't have permission to view this document.");
            }

            return _mapper.Map<DocumentDto>(document);
        }

        private bool ViolateConstraintForStaffAndEmployee(User user, Document document)
            => !IsInSameDepartment(user, document)
               && document.IsPrivate
               && ( IsStaffAndNotInSameRoom(user, document)
               || IsEmployeeAndDoesNotHasReadPermission(user, document) );
        
        private bool IsStaffAndNotInSameRoom(User user, Document document)
        {
            var staff = _context.Staffs.FirstOrDefault(x => x.Id == user.Id);
            return user.Role.IsStaff()  
                    && staff is not null 
                    && !staff.Room!.Id.Equals(document.Folder?.Locker.Room.Id);
        }

        private bool IsEmployeeAndDoesNotHasReadPermission(User user, Document document)
            => user.Role.IsEmployee()
               && document.ImporterId != user.Id
               && !_permissionManager.IsGranted(document.Id, DocumentOperation.Read, user.Id);

        private bool IsInSameDepartment(User user, Document document)
            => user.Department == document.Department;
    }
}