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

namespace Application.Entries.Queries;

public class GetAllSharedEntriesPaginated
{
    public record Query : IRequest<PaginatedList<EntryDto>>
    {
        public User CurrentUser { get; init; } = null!;
        public int? Page { get; init; }
        public int? Size { get; init; }
        public string? SortBy { get; init; }
        public string? SortOrder { get; init; }
        public Guid? EntryId { get; init; }
    }

    public class QueryHandler : IRequestHandler<Query, PaginatedList<EntryDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        
        public QueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaginatedList<EntryDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var permissions = _context.EntryPermissions
                .Include(x => x.Entry)
                .ThenInclude(y => y.Uploader)
                .Include(x => x.Entry)
                .ThenInclude(y => y.Owner)
                .Where(x =>
                    x.EmployeeId == request.CurrentUser.Id
                    && x.AllowedOperations
                        .Contains(EntryOperation.View.ToString()));

            IQueryable<Entry> entries;
            if (request.EntryId is null)
            {
                entries = permissions
                    .Where(x => x.IsSharedRoot)
                    .Select(x => x.Entry);
            }
            else
            {
                var baseEntry = await permissions
                    .Select(x => x.Entry)
                    .FirstOrDefaultAsync(x => x.Id == request.EntryId, cancellationToken);

                if (baseEntry is null)
                {
                    throw new KeyNotFoundException("Base entry does not exist.");
                }
                
                var basePath = baseEntry.Path.Equals("/")
                    ? $"{baseEntry.Path}{baseEntry.Name}"
                    : $"{baseEntry.Path}/{baseEntry.Name}";
                var pattern = $"{baseEntry.Path}/";

                entries = permissions
                    .Select(x => x.Entry)
                    .Where(x => x.Path.Equals(basePath)
                                || EF.Functions.Like(x.Path, pattern));
            }
            return await entries
                .ListPaginateWithSortAsync<Entry, EntryDto>(
                    request.Page,
                    request.Size,
                    request.SortBy,
                    request.SortOrder,
                    _mapper.ConfigurationProvider,
                    cancellationToken);
        }
    }
}