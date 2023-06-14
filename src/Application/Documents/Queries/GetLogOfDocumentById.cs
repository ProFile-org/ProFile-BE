using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Logging;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Documents.Queries;

public class GetLogOfDocumentById
{
    public record Query : IRequest<DocumentLogDto>
    {
        public Guid LogId { get; init; }
    }

    public class QueryHandler : IRequestHandler<Query, DocumentLogDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public QueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<DocumentLogDto> Handle(Query request, CancellationToken cancellationToken)
        {
            var log = await _context.DocumentLogs
                .Include(x => x.Object)
                .Include(x => x.User)
                .ThenInclude(x => x.Department)
                .FirstOrDefaultAsync(x => x.Id.Equals(request.LogId), cancellationToken);

            if (log is null)
            {
                throw new KeyNotFoundException("Log does not exist.");
            }

            return _mapper.Map<DocumentLogDto>(log);
        }
    }
}