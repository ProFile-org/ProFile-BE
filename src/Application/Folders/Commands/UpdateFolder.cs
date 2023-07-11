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
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Application.Folders.Commands;

public class UpdateFolder
{
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(f => f.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(64).WithMessage("Name cannot exceed 64 characters.");

            RuleFor(f => f.Description)
                .MaximumLength(256).WithMessage("Description cannot exceed 256 characters.");

            RuleFor(f => f.Capacity)
                .NotEmpty().WithMessage("Folder capacity is required.")
                .GreaterThanOrEqualTo(1).WithMessage("Folder's capacity cannot be less than 1.");
        }
    }
    
    public record Command : IRequest<FolderDto>
    {
        public User CurrentUser { get; init; } = null!;
        public Guid? CurrentStaffRoomId { get; init; }
        public Guid FolderId { get; init; }
        public string Name { get; init; } = null!;
        public string? Description { get; init; }
        public int Capacity { get; init; }
        public bool IsAvailable { get; init; }
    }

    public class CommandHandler : IRequestHandler<Command, FolderDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger<UpdateFolder> _logger;

        public CommandHandler(IApplicationDbContext context, IMapper mapper, IDateTimeProvider dateTimeProvider, ILogger<UpdateFolder> logger)
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
                && (request.CurrentStaffRoomId is null || !FolderIsInRoom(folder, request.CurrentStaffRoomId!.Value)))
            {
                throw new UnauthorizedAccessException("User cannot access this resource.");
            }
            
            if (await DuplicatedNameFolderExistsInSameLockerAsync(request.Name, folder.Id, folder.Locker.Id, cancellationToken))
            {
                throw new ConflictException("Folder name already exists.");
            }

            if (request.Capacity < folder.NumberOfDocuments)
            {
                throw new ConflictException("New capacity cannot be less than current number of documents.");
            }

            var localDateTimeNow = LocalDateTime.FromDateTime(_dateTimeProvider.DateTimeNow);
            
            folder.Name = request.Name;
            folder.Description = request.Description;
            folder.Capacity = request.Capacity;
            folder.LastModified = localDateTimeNow;
            folder.LastModifiedBy = request.CurrentUser.Id;
            folder.IsAvailable = request.IsAvailable;
            
            var log = new FolderLog()
            {
                User = request.CurrentUser,
                UserId = request.CurrentUser.Id,
                ObjectId = folder.Id,
                Time = localDateTimeNow,
                Action = FolderLogMessage.Update,
            };
            var result = _context.Folders.Update(folder);
            await _context.FolderLogs.AddAsync(log, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            using (Logging.PushProperties(nameof(Folder), folder.Id, request.CurrentUser.Id))
            {
                _logger.LogAddFolder(folder.Id.ToString(),
                    folder.Locker.Id.ToString(),
                    folder.Locker.Room.Id.ToString(),
                    folder.Locker.Room.Department.Name);
            }

            return _mapper.Map<FolderDto>(result.Entity);
        }
        private async Task<bool> DuplicatedNameFolderExistsInSameLockerAsync(
            string folderName,
            Guid lockerId,
            Guid folderId,
            CancellationToken cancellationToken)
        {
            var folder = await _context.Folders.FirstOrDefaultAsync(
                x => x.Name.Trim().ToLower().Equals(folderName.Trim().ToLower())
                                    && x.Id != folderId
                                    && x.Locker.Id == lockerId,
                cancellationToken);
            return folder is not null;
        }
        
        private static bool FolderIsInRoom(Folder folder, Guid roomId)
            => folder.Locker.Room.Id == roomId;
    }
}