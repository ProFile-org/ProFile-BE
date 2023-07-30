using Application.Common.Exceptions;
using Application.Common.Extensions;
using Application.Common.Extensions.Logging;
using Application.Common.Interfaces;
using Application.Common.Logging;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Physical;
using Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Application.Folders.Commands;

public class AddFolder
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

            RuleFor(f => f.LockerId)
                .NotEmpty().WithMessage("LockerId is required.");
        }
    }

    public record Command : IRequest<FolderDto>
    {
        public User CurrentUser { get; init; } = null!;
        public Guid? CurrentStaffRoomId { get; init; }
        public string Name { get; init; } = null!;
        public string? Description { get; init; }
        public int Capacity { get; init; }
        public Guid LockerId { get; init; }
    }

    public class CommandHandler : IRequestHandler<Command, FolderDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger<AddFolder> _logger;

        public CommandHandler(IApplicationDbContext context, IMapper mapper, IDateTimeProvider dateTimeProvider, ILogger<AddFolder> logger)
        {
            _context = context;
            _mapper = mapper;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
        }

        public async Task<FolderDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var locker = await _context.Lockers
                .Include(x => x.Room)
                .ThenInclude(y => y.Department)
                .FirstOrDefaultAsync(l => l.Id == request.LockerId, cancellationToken);

            if (locker is null)
            {
                throw new KeyNotFoundException("Locker does not exist.");
            }

            if (locker.NumberOfFolders >= locker.Capacity)
            {
                throw new LimitExceededException("This locker cannot accept more folders.");
            }

            if (request.CurrentUser.Role.IsStaff()
                && (locker.Room.Id != request.CurrentStaffRoomId
                    || !LockerIsInRoom(locker, request.CurrentStaffRoomId)))
            {
                throw new UnauthorizedAccessException("User cannot access this resource.");
            }

            if (await DuplicatedNameFolderExistsInSameLockerAsync(request.Name, locker.Id, cancellationToken))
            {
                throw new ConflictException("Folder name already exists.");
            }
            
            var localDateTimeNow = LocalDateTime.FromDateTime(_dateTimeProvider.DateTimeNow);
            
            var entity = new Folder
            {
                Name = request.Name.Trim(),
                Description = request.Description,
                NumberOfDocuments = 0,
                Capacity = request.Capacity,
                Locker = locker,
                IsAvailable = true,
                Created = localDateTimeNow,
                CreatedBy = request.CurrentUser.Id,
            };
            
            var result = await _context.Folders.AddAsync(entity, cancellationToken);
            locker.NumberOfFolders += 1;
            _context.Lockers.Update(locker);
            await _context.SaveChangesAsync(cancellationToken);

            using (Logging.PushProperties(nameof(Folder), result.Entity.Id, request.CurrentUser.Id))
            {
                _logger.LogAddFolder(result.Entity.Id.ToString(),
                    result.Entity.Locker.Id.ToString(),
                    locker.Room.Id.ToString(),
                    locker.Room.Department.Name);
            }

            return _mapper.Map<FolderDto>(result.Entity);
        }
        
        private async Task<bool> DuplicatedNameFolderExistsInSameLockerAsync(string folderName, Guid lockerId, CancellationToken cancellationToken)
        {
            var folder = await _context.Folders.FirstOrDefaultAsync(
                x => x.Name.ToLower().Equals(folderName.ToLower())
                     && x.Locker.Id == lockerId, cancellationToken);
            return folder is not null;
        }

        private static bool LockerIsInRoom(Locker locker, Guid? roomId)
            => roomId is not null && locker.Room.Id == roomId;
    }
}