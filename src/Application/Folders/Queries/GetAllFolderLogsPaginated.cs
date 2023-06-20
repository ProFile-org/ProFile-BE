using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Dtos.Logging;
using AutoMapper;
using Domain.Entities.Logging;
using Domain.Entities.Physical;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Folders.Queries;

public class GetAllFolderLogsPaginated
{
    public record Query : IRequest<PaginatedList<FolderLogDto>>
    {
        public Guid? FolderId { get; init; }
        public string? SearchTerm { get; init; }
        public int? Page { get; init; }
        public int? Size { get; init; }
    }

    public class QueryHandler : IRequestHandler<Query, PaginatedList<FolderLogDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public QueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaginatedList<FolderLogDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var logs = _context.FolderLogs
                .Include(x => x.Object)
                .AsQueryable();
            
            if (request.FolderId is not null)
            {
                logs = logs.Where(x => x.Object!.Id == request.FolderId);
            }

            if (!(request.SearchTerm is null || request.SearchTerm.Trim().Equals(string.Empty)))
            {
                logs = logs.Where(x => 
                    x.Action.Trim().ToLower().Contains(request.SearchTerm.Trim().ToLower()));
            }


            return await logs
                .LoggingListPaginateAsync<Folder, FolderLog, FolderLogDto>(
                    request.Page,
                    request.Size,
                    _mapper.ConfigurationProvider,
                    cancellationToken);
        }
    }
}