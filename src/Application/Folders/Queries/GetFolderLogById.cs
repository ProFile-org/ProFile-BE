using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Logging;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Folders.Queries;

public class GetFolderLogById
{
    public record Query : IRequest<FolderLogDto>
    {
        public Guid LogId { get; init; }
    }

    public class QueryHandler : IRequestHandler<Query, FolderLogDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public QueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<FolderLogDto> Handle(Query request, CancellationToken cancellationToken)
        {
            var log = await _context.FolderLogs
                .Include(x => x.Object)
                .Include(x => x.User)
                .ThenInclude(x => x.Department)
                .FirstOrDefaultAsync(x => x.Id.Equals(request.LogId), cancellationToken);
            
            if (log is null)
            {
                throw new KeyNotFoundException("Log does not exist.");
            }

            return _mapper.Map<FolderLogDto>(log);
        }
    }
}