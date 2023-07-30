using Application.Common.Exceptions;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Dtos.Digital;
using Application.Common.Models.Operations;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Digital;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Queries;

public class GetAllSharedUsersOfASharedEntryPaginated
{
    public record Query : IRequest<PaginatedList<EntryPermissionDto>>
    {
        public User CurrentUser { get; init; } = null!;
        public Guid EntryId { get; init; }
        public string? SearchTerm { get; init; }
        public int? Page { get; init; }
        public int? Size { get; init; }
    }

    public class Handler : IRequestHandler<Query, PaginatedList<EntryPermissionDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public Handler(IMapper mapper, IApplicationDbContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<PaginatedList<EntryPermissionDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var entry = await _context.Entries
                .FirstOrDefaultAsync(x => x.Id == request.EntryId, cancellationToken);

            if (entry is null)
            {
                throw new KeyNotFoundException("Entry does not exist.");
            }

            var permission = await _context.EntryPermissions
                .FirstOrDefaultAsync(x => x.EntryId == request.EntryId
                                          && x.EmployeeId == request.CurrentUser.Id, cancellationToken);

            if ((permission is null || !permission.AllowedOperations.Contains(EntryOperation.View.ToString())) &&
                (entry.OwnerId != request.CurrentUser.Id))
            {
                throw new UnauthorizedAccessException("You do not have permission to view this shared entry's users.");
            }

            var permissions = _context.EntryPermissions
                .Include(x => x.Employee)
                .Where(x => x.EntryId == request.EntryId);
            
            if (!(request.SearchTerm is null || request.SearchTerm.Trim().Equals(string.Empty)))
            {
                permissions = permissions.Where(x =>
                    x.Employee.Username.ToLower().Trim().Contains(request.SearchTerm.ToLower().Trim()));
            }
            
            var pageNumber = request.Page is null or <= 0 ? 1 : request.Page;
            var sizeNumber = request.Size is null or <= 0 ? 10 : request.Size;
            
            var count = await permissions.CountAsync(cancellationToken);
            var list  = await permissions
                .Paginate(pageNumber.Value, sizeNumber.Value)
                .ToListAsync(cancellationToken);

            var result = _mapper.Map<List<EntryPermissionDto>>(list);
            return new PaginatedList<EntryPermissionDto>(result, count, pageNumber.Value, sizeNumber.Value);
        }
    }
}