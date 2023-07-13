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
        public string EntryPath { get; init; } = null!;
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
            var path = $"{request.CurrentUser.Username}_bin";
            var count1 = path.Length;

            if (!request.EntryPath.Equals("/"))
            {
                path += request.EntryPath;
            }

            var entries = _context.Entries
                .Include(x => x.Owner)
                .Include(x => x.File)
                .Where(x => x.Owner.Id == request.CurrentUser.Id
                && x.Path == path);

            
            // var realPath = $"{request.CurrentUser.Username}_bin{request.EntryPath}";


            if (!(request.SearchTerm is null || request.SearchTerm.Trim().Equals(string.Empty)))
            {
                entries = entries.Where(x =>
                    x.Name.Contains(request.SearchTerm.Trim()));
            }

            var sortBy = request.SortBy;
            
            if (sortBy is null || !sortBy.MatchesPropertyName<EntryDto>())
            {
                sortBy = nameof(EntryDto.Id);
            }

            var sortOrder = request.SortOrder ?? "asc";
            var pageNumber = request.Page is null or <= 0 ? 1 : request.Page;
            var sizeNumber = request.Size is null or <= 0 ? 10 : request.Size;
            
            var count = await entries.CountAsync(cancellationToken);
            var list  = await entries
                .OrderByCustom(sortBy, sortOrder)
                .Paginate(pageNumber.Value, sizeNumber.Value)
                .ToListAsync(cancellationToken);
            
            list.ForEach(x => { x.Path = x.Path.Length == count1 ? "/" : x.Path[count1..]; });

            var result = _mapper.Map<List<EntryDto>>(list);
            return new PaginatedList<EntryDto>(result, count, pageNumber.Value, sizeNumber.Value);
        }
    }
}