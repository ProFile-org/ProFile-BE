using System.Collections.ObjectModel;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Documents.Queries;

public class GetAllDocumentTypes
{
    public record Query : IRequest<IEnumerable<string>>;

    public class QueryHandler : IRequestHandler<Query, IEnumerable<string>>
    {
        private readonly IApplicationDbContext _context;

        public QueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<string>> Handle(Query request, CancellationToken cancellationToken)
        {
            return new ReadOnlyCollection<string>(await _context.Documents.Select(x => x.DocumentType).Distinct()
                .ToListAsync(cancellationToken));
        }
    } 
}