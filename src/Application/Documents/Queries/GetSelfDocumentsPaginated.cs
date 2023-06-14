using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Documents.Queries;

public class GetSelfDocumentsPaginated
{
    public record Query : IRequest<PaginatedList<DocumentDto>>
    {
        public Guid EmployeeId { get; init; }
        public string? SearchTerm { get; set; }
        public int? Page { get; init; }
        public int? Size { get; init; }
        public string? SortBy { get; init; }
        public string? SortOrder { get; init; }

        public class QueryHandler : IRequestHandler<Query, PaginatedList<DocumentDto>>
        {
            private readonly IApplicationDbContext _context;
            private readonly IMapper _mapper;

            public QueryHandler(IApplicationDbContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<PaginatedList<DocumentDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                var documents = _context.Documents.AsQueryable();

                documents = documents
                    .Include(x => x.Department)
                    .Where(x => x.Importer!.Id.Equals(request.EmployeeId));

                if (!(request.SearchTerm is null || request.SearchTerm.Trim().Equals(string.Empty)))
                {
                    documents = documents.Where(x =>
                        x.Title.ToLower().Contains(request.SearchTerm.ToLower()));
                }
                
                var sortBy = request.SortBy;
                if (sortBy is null || !sortBy.MatchesPropertyName<DocumentDto>())
                {
                    sortBy = nameof(DocumentDto.Id);
                }
                var sortOrder = request.SortOrder ?? "asc";
                var pageNumber = request.Page is null or <= 0 ? 1 : request.Page;
                var sizeNumber = request.Size is null or <= 0 ? 5 : request.Size;
            
                var count = await documents.CountAsync(cancellationToken);
                var list  = await documents
                    .OrderByCustom(sortBy, sortOrder)
                    .Paginate(pageNumber.Value, sizeNumber.Value)
                    .ToListAsync(cancellationToken);
            
                var result = _mapper.Map<List<DocumentDto>>(list);

                return new PaginatedList<DocumentDto>(result, count, pageNumber.Value, sizeNumber.Value);
            }
        }
    }
}