using Application.Common.Exceptions;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Queries;

public class GetAllSharedUsersOfASharedEntryPaginated
{
    public record Query : IRequest<PaginatedList<UserDto>>
    {
        public User CurrentUser { get; init; } = null!;
        public Guid EntryId { get; init; }
        public string? SearchTerm { get; init; }
        public int? Page { get; init; }
        public int? Size { get; init; }
        public string? SortBy { get; init; }
        public string? SortOrder { get; init; }
    }

    public class Handler : IRequestHandler<Query, PaginatedList<UserDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public Handler(IMapper mapper, IApplicationDbContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<PaginatedList<UserDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var entry = await _context.Entries
                .FirstOrDefaultAsync(x => x.Id == request.EntryId, cancellationToken);

            if (entry is null)
            {
                throw new KeyNotFoundException("Entry does not exist.");
            }

            if (entry.OwnerId != request.CurrentUser.Id)
            {
                throw new NotAllowedException("You do not have permission to view shared users of this entry.");
            }
            
            var sharedUsers = _context.EntryPermissions
                .AsQueryable()
                .Where(x => x.EntryId == request.EntryId)
                .Select(x => x.Employee);
            
            if (!(request.SearchTerm is null || request.SearchTerm.Trim().Equals(string.Empty)))
            {
                sharedUsers = sharedUsers.Where(x =>
                    x.Username.ToLower().Trim().Contains(request.SearchTerm.ToLower().Trim()));
            }
            
            return await sharedUsers
                .ListPaginateWithSortAsync<User, UserDto>(
                    request.Page,
                    request.Size,
                    request.SortBy,
                    request.SortOrder,
                    _mapper.ConfigurationProvider,
                    cancellationToken);
        }
    }
}