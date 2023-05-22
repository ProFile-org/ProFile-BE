using System.Collections.ObjectModel;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Documents.Queries.GetDocumentTypes;

public record GetAllDocumentTypesQuery : IRequest<IEnumerable<string>>;

public class GetAllDocumentTypesQueryHandler : IRequestHandler<GetAllDocumentTypesQuery, IEnumerable<string>>
{
    private readonly IApplicationDbContext _context;

    public GetAllDocumentTypesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<IEnumerable<string>> Handle(GetAllDocumentTypesQuery request, CancellationToken cancellationToken)
    {
        return new ReadOnlyCollection<string>(await _context.Documents.Select(x => x.DocumentType)
            .ToListAsync(cancellationToken));
    }
} 