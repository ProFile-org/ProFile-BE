using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Folders.Queries;

public class GetFolderById
{
    public record Query : IRequest<FolderDto>
    {
        public Guid FolderId { get; init; }
    }

    public class QueryHandler : IRequestHandler<Query, FolderDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public QueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<FolderDto> Handle(Query request, CancellationToken cancellationToken)
        {
            var folder = await _context.Folders
                .Include(x =>x .Locker)
                .ThenInclude(x => x.Room)
                .FirstOrDefaultAsync(x => x.Id.Equals(request.FolderId), cancellationToken);

            if (folder is null)
            {
                throw new KeyNotFoundException("Folder does not exist.");
            }

            return _mapper.Map<FolderDto>(folder);
        }
    }
}