using Application.Common.Exceptions;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Logging;
using Application.Common.Messages;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Logging;
using Domain.Entities.Physical;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Application.Folders.Commands;

public class RemoveFolder
{
    public record Command : IRequest<FolderDto>
    {
        public User CurrentUser { get; init; } = null!;
        public Guid? CurrentStaffRoomId { get; init; }
        public Guid FolderId { get; init; }
    }
    
    public class CommandHandler : IRequestHandler<Command, FolderDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger<RemoveFolder> _logger;

        public CommandHandler(IApplicationDbContext context, IMapper mapper, IDateTimeProvider dateTimeProvider, ILogger<RemoveFolder> logger)
        {
            _context = context;
            _mapper = mapper;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
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

            if (request.CurrentUser.Role.IsStaff()
                && (request.CurrentStaffRoomId is null || !FolderIsInRoom(folder, request.CurrentStaffRoomId.Value)))
            {
                throw new UnauthorizedAccessException("User cannot remove this resource.");
            }

            var canNotRemove = folder.NumberOfDocuments > 0;

            if (canNotRemove)
            {
                throw new ConflictException("Folder cannot be removed because it contains documents.");
            }

            var localDateTimeNow = LocalDateTime.FromDateTime(_dateTimeProvider.DateTimeNow);
            
            var locker = folder.Locker;

            var log = new FolderLog()
            {
                User = request.CurrentUser,
                UserId = request.CurrentUser.Id,
                ObjectId = folder.Id,
                Time = localDateTimeNow,
                Action = FolderLogMessage.Remove,
            };
            var result = _context.Folders.Remove(folder);
            locker.NumberOfFolders -= 1;
            await _context.FolderLogs.AddAsync(log, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            using (Logging.PushProperties(nameof(Folder), folder.Id, request.CurrentUser.Id))
            {
                _logger.LogRemoveFolder(folder.Id.ToString(),
                    folder.Locker.Id.ToString(),
                    folder.Locker.Room.Id.ToString(),
                    folder.Locker.Room.Department.Name);
            }

            return _mapper.Map<FolderDto>(result.Entity);
        }

        private static bool FolderIsInRoom(Folder folder, Guid roomId)
            => folder.Locker.Room.Id == roomId;
    }
}