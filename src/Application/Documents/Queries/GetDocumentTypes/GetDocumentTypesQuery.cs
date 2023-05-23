using System.Collections.ObjectModel;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Documents.Queries.GetDocumentTypes;

public record GetDocumentTypesQuery : IRequest<IEnumerable<string>>;

public class GetDocumentTypesQueryHandler : IRequestHandler<GetDocumentTypesQuery, IEnumerable<string>>
{
    private readonly IApplicationDbContext _context;

    public GetDocumentTypesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<IEnumerable<string>> Handle(GetDocumentTypesQuery request, CancellationToken cancellationToken)
    {
        return new ReadOnlyCollection<string>(await _context.Documents.Select(x => x.DocumentType)
            .ToListAsync(cancellationToken));
    }
} 