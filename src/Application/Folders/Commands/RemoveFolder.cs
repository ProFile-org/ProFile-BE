using Application.Common.Exceptions;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using Domain.Entities.Physical;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Folders.Commands;

public class RemoveFolder
{
    public record Command : IRequest<FolderDto>
    {
        public string CurrentUserRole { get; init; } = null!;
        public Guid CurrentUserDepartmentId { get; init; }
        public Guid FolderId { get; init; }
    }
    
    public class CommandHandler : IRequestHandler<Command, FolderDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CommandHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<FolderDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var folder = await _context.Folders
                .Include(x => x.Locker)
                .ThenInclude(x => x.Room)
                .ThenInclude(x => x.Department)
                .FirstOrDefaultAsync(x => x.Id.Equals(request.FolderId), cancellationToken);

            if (folder is null)
            {
                throw new KeyNotFoundException("Folder does not exist.");
            }

            if (request.CurrentUserRole.IsStaff()
                && !FolderIsInDepartment(folder, request.CurrentUserDepartmentId))
            {
                throw new UnauthorizedAccessException("User cannot remove this resource.");
            }

            var canNotRemove = folder.NumberOfDocuments > 0;

            if (canNotRemove)
            {
                throw new ConflictException("Folder cannot be removed because it contains documents.");
            }

            var locker = folder.Locker;
            var result = _context.Folders.Remove(folder);
            locker.NumberOfFolders -= 1;
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<FolderDto>(result.Entity);
        }

        private static bool FolderIsInDepartment(Folder folder, Guid departmentId)
            => folder.Locker.Room.DepartmentId == departmentId;
    }
}