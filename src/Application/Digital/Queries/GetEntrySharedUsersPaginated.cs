using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Dtos;
using Application.Common.Models.Operations;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Digital.Queries;

public abstract class GetEntrySharedUsersPaginated
{
    public record Query : IRequest<PaginatedList<Result>>
    {
        public User CurrentUser { get; init; } = null!;
        public Guid EntryId { get; init; }
        public int? Page { get; init; }
        public int? Size { get; init; }
    }

    public class Result : BaseDto
    {
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DepartmentDto? Department { get; set; }
        public string Role { get; set; } = null!;
        public string? Position { get; set; }
        public bool IsActive { get; set; }
        public bool IsActivated { get; set; }
        public IEnumerable<string> AllowedOperations { get; set; } = new List<string>();
    }

    public class QueryHandler : IRequestHandler<Query, PaginatedList<Result>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        
        public QueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaginatedList<Result>> Handle(Query request, CancellationToken cancellationToken)
        {
            var entry = await _context.Entries.FirstOrDefaultAsync(x => x.Id == request.EntryId, cancellationToken);

            if (entry is null)
            {
                throw new KeyNotFoundException("Entry does not exist.");
            }
            
            if (entry.OwnerId != request.CurrentUser.Id)
            {
                var currentUserPermission = await _context.EntryPermissions.FirstOrDefaultAsync(
                    x => x.EntryId == entry.Id
                         && x.EmployeeId == request.CurrentUser.Id, cancellationToken);

                if (currentUserPermission is null
                    || !currentUserPermission.AllowedOperations
                        .Split(",")
                        .Contains(EntryOperation.ChangePermission.ToString()))
                {
                    throw new UnauthorizedAccessException("User cannot access this resource.");
                }
            }

            var permissions = _context.EntryPermissions.Where(x => x.EntryId == entry.Id);

            var pageNumber = request.Page is null or <= 0 ? 1 : request.Page;
            var sizeNumber = request.Size is null or <= 0 ? 10 : request.Size;
            
            var count = await permissions.CountAsync(cancellationToken);
            var result = await permissions
                .OrderBy(x => x.ExpiryDateTime)
                .Select(x => new Result()
                {
                    Id = x.Employee.Id,
                    Username = x.Employee.Username,
                    Email = x.Employee.Email,
                    FirstName = x.Employee.FirstName,
                    LastName = x.Employee.LastName,
                    Department = _mapper.Map<DepartmentDto>(x.Employee.Department),
                    Role = x.Employee.Role,
                    Position = x.Employee.Position,
                    IsActive = x.Employee.IsActive,
                    IsActivated = x.Employee.IsActivated,
                    AllowedOperations = x.AllowedOperations.Split(",", StringSplitOptions.RemoveEmptyEntries),
                })
                .Paginate(pageNumber.Value, sizeNumber.Value)
                .ToListAsync(cancellationToken);
            return new PaginatedList<Result>(result, count, pageNumber.Value, sizeNumber.Value);
        }
    }
}