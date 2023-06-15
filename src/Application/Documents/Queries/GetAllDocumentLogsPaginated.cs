using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Dtos.Logging;
using AutoMapper;
using Domain.Entities.Logging;
using Domain.Entities.Physical;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Documents.Queries;

public class GetAllDocumentLogsPaginated
{
    public record Query : IRequest<PaginatedList<DocumentLogDto>>
    {
        public string? SearchTerm { get; init; }
        public int? Page { get; init; }
        public int? Size { get; init; }
    }

    public class QueryHandler : IRequestHandler<Query, PaginatedList<DocumentLogDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public QueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaginatedList<DocumentLogDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var logs = _context.DocumentLogs
                .Include(x => x.Object)
                .Include(x => x.User)
                .ThenInclude(x => x.Department)
                .AsQueryable();

            if (!(request.SearchTerm is null || request.SearchTerm.Trim().Equals(string.Empty)))
            {
                logs = logs.Where(x =>
                    x.Action.ToLower().Contains(request.SearchTerm.ToLower()));
            }

            return await logs
                .LoggingListPaginateAsync<Document, DocumentLog, DocumentLogDto>(
                    request.Page,
                    request.Size,
                    _mapper.ConfigurationProvider,
                    cancellationToken);
        }
    }
}