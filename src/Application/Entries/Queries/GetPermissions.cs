using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Digital;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Digital;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Entries.Queries;

public class GetPermissions
{
    public record Query : IRequest<EntryPermissionDto>
    {
        public User CurrentUser { get; init; } = null!;
        public Guid EntryId { get; init; }
    }
    
    public class QueryHandler : IRequestHandler<Query, EntryPermissionDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public QueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<EntryPermissionDto> Handle(Query request, CancellationToken cancellationToken)
        {
            var entry = await _context.Entries
                .Include(x => x.File)
                .FirstOrDefaultAsync(x => x.Id == request.EntryId, cancellationToken);

            if (entry is null)
            {
                throw new ConflictException("Entry does not exist.");
            }

            if (entry.OwnerId == request.CurrentUser.Id)
            {
                return CreateEntryPermissionDto(entry.Id, entry.OwnerId,
                    true, true, false);
            }

            var permission = await _context.EntryPermissions
                .FirstOrDefaultAsync(x => 
                    x.EntryId == entry.Id 
                    && x.EmployeeId == request.CurrentUser.Id, cancellationToken);

            if (permission is null)
            {
                return CreateEntryPermissionDto(entry.Id, request.CurrentUser.Id,
                    false, false, false);
            }

            return _mapper.Map<EntryPermissionDto>(permission);
        }

        private static EntryPermissionDto CreateEntryPermissionDto(
            Guid entryId, Guid userId,
            bool canView, bool canEdit,
            bool isSharedRoot)
        {
            return new EntryPermissionDto
            {
                EmployeeId = userId,
                EntryId = entryId,
                CanView = canView,
                CanEdit = canEdit,
                IsSharedRoot = isSharedRoot,
            };
        }
    }
}