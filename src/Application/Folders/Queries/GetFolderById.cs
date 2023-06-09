using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using Domain.Entities.Physical;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Folders.Queries;

public class GetFolderById
{
    public record Query : IRequest<FolderDto>
    {
        public string CurrentUserRole { get; init; } = null!;
        public Guid? CurrentStaffRoomId { get; init; }
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
                .ThenInclude(x => x.Department)
                .FirstOrDefaultAsync(x => x.Id.Equals(request.FolderId), cancellationToken);

            if (folder is null)
            {
                throw new KeyNotFoundException("Folder does not exist.");
            }
            
            if (request.CurrentUserRole.IsStaff()
                && (request.CurrentStaffRoomId is null || !FolderInSameRoom(folder, request.CurrentStaffRoomId.Value)))
            {
                throw new UnauthorizedAccessException("User cannot access this resource.");
            }

            return _mapper.Map<FolderDto>(folder);
        }
        
        private static bool FolderInSameRoom(Folder folder, Guid roomId)
            => folder.Locker.Room.Id == roomId;
    }
}