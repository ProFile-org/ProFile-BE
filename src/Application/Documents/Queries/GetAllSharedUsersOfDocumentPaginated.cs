using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Dtos.Physical;
using Application.Common.Models.Operations;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Documents.Queries;

public class GetAllSharedUsersOfDocumentPaginated
{
    public record Query : IRequest<PaginatedList<PermissionDto>>
    {
        public User CurrentUser { get; init; } = null!;
        public Guid DocumentId { get; init; }
        public string? SearchTerm { get; init; }
        public int? Page { get; init; }
        public int? Size { get; init; }
    }
    
    public class Handler : IRequestHandler<Query, PaginatedList<PermissionDto>>
    {
        
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public Handler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaginatedList<PermissionDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var document = await _context.Documents
                .FirstOrDefaultAsync(x => x.Id == request.DocumentId, cancellationToken);

            if (document is null)
            {
                throw new KeyNotFoundException("Document does not exist.");
            }

            var permission = await _context.Permissions
                .FirstOrDefaultAsync(x => x.DocumentId == request.DocumentId
                                          && x.EmployeeId == request.CurrentUser.Id, cancellationToken);
            if ((permission is null || !permission.AllowedOperations.Contains(DocumentOperation.Read.ToString()))
                && document.ImporterId != request.CurrentUser.Id)
            {
                throw new UnauthorizedAccessException("You do not have permission to view this document shared users.");
            }

            var permissions = _context.Permissions
                .Include(x => x.Employee)
                .Where(x => x.DocumentId == request.DocumentId);

            if (request.SearchTerm is not null && !request.SearchTerm.Trim().Equals(string.Empty))
            {
                permissions = permissions.Where(x =>
                    x.Employee.Username.Trim().ToLower().Contains(request.SearchTerm.Trim().ToLower())
                    || x.Employee.Email.Trim().ToLower().Contains(request.SearchTerm.Trim().ToLower()));
            }
            
            var pageNumber = request.Page is null or <= 0 ? 1 : request.Page;
            var sizeNumber = request.Size is null or <= 0 ? 10 : request.Size;

            var list  = await permissions
                .Paginate(pageNumber.Value, sizeNumber.Value)
                .ToListAsync(cancellationToken);
            
            var count = await permissions.CountAsync(cancellationToken);

            var result = _mapper.Map<List<PermissionDto>>(list);
            return new PaginatedList<PermissionDto>(result, count, pageNumber.Value, sizeNumber.Value);
        }
    }
}