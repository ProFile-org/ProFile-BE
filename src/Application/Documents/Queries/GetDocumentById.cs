using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Documents.Queries;

public class GetDocumentById
{
    public record Query : IRequest<DocumentDto>
    {
        public Guid DocumentId { get; init; } 
    }

    public class QueryHandler : IRequestHandler<Query, DocumentDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public QueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<DocumentDto> Handle(Query request, CancellationToken cancellationToken)
        {
            var document = await _context.Documents
                .Include(x => x.Department)
                .Include(x => x.Importer)
                .Include(x => x.Folder)
                .ThenInclude(y => y.Locker)
                .ThenInclude(z => z.Room)
                .FirstOrDefaultAsync(x => x.Id == request.DocumentId, cancellationToken);

            if (document is null)
            {
                throw new KeyNotFoundException("Document does not exist.");
            }
        
            return _mapper.Map<DocumentDto>(document);
        }
    }
}