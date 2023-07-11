using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Dtos.Digital;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Digital;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Entries.Queries;

public class GetAllBinEntriesPaginated
{
    public record Query : IRequest<PaginatedList<EntryDto>>
    {
        public User CurrentUser { get; init; } = null!;
        public string? SearchTerm { get; init; }
        public int? Page { get; init; }
        public int? Size { get; init; }
        public string? SortBy { get; init; }
        public string? SortOrder { get; init; }
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
            var pattern = $"{request.CurrentUser.Username}_bin/%";

            var entries = _context.Entries
                .Include(x => x.Owner)
                .AsQueryable()
                .Where(x => x.Owner.Username.Equals(request.CurrentUser.Username)
                && EF.Functions.Like(x.Path, pattern));

            if (!(request.SearchTerm is null || request.SearchTerm.Trim().Equals(string.Empty)))
            {
                entries = entries.Where(x =>
                    x.Name.Contains(request.SearchTerm.Trim()));
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