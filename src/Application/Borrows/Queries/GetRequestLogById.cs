using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Logging;
using AutoMapper;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Borrows.Queries;

public class GetRequestLogById
{
    public record Query : IRequest<RequestLogDto>
    {
        public Guid LogId { get; init; }
    }

    public class QueryHandler : IRequestHandler<Query, RequestLogDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public QueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<RequestLogDto> Handle(Query request, CancellationToken cancellationToken)
        {
            var log = await _context.RequestLogs
                .Include(x => x.Object)
                .ThenInclude(x => x!.Importer)
                .Include(x => x.Object)
                .ThenInclude(x => x!.Folder)
                .Include(x => x.User)
                .ThenInclude(x => x.Department)
                .FirstOrDefaultAsync(x => x.Id.Equals(request.LogId), cancellationToken);

            if (log is null)
            {
                throw new KeyNotFoundException("Log does not exist.");
            }

            return _mapper.Map<RequestLogDto>(log);
        }
    }
}