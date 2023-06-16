using Application.Common.Exceptions;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Messages;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Logging;
using Domain.Entities.Physical;
using Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
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

        public CommandHandler(IApplicationDbContext context, IMapper mapper, IDateTimeProvider dateTimeProvider)
        {
            _context = context;
            _mapper = mapper;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<FolderDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var locker = await _context.Lockers
                .Include(x => x.Room)
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
                && !LockerExistsAndInSameDepartment(locker, request.CurrentUser.Department?.Id))
            {
                throw new UnauthorizedAccessException("User cannot add this resource.");
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
            
            var log = new FolderLog()
            {
                User = request.CurrentUser,
                UserId = request.CurrentUser.Id,
                Object = entity,
                Time = localDateTimeNow,
                Action = FolderLogMessage.Add,
            };
            var result = await _context.Folders.AddAsync(entity, cancellationToken);
            locker.NumberOfFolders += 1;
            _context.Lockers.Update(locker);
            await _context.FolderLogs.AddAsync(log, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<FolderDto>(result.Entity);
        }
        
        private async Task<bool> DuplicatedNameFolderExistsInSameLockerAsync(string folderName, Guid lockerId, CancellationToken cancellationToken)
        {
            var folder = await _context.Folders.FirstOrDefaultAsync(
                x => EqualsInvariant(x.Name, folderName) 
                     && IsSameLocker(x.Locker.Id, lockerId), cancellationToken);
            return folder is not null;
        }

        private static bool EqualsInvariant(string x, string y)
            => x.Trim().ToLower().Equals(y.Trim().ToLower());

        private static bool IsSameLocker(Guid lockerId1, Guid lockerId2)
            => lockerId1 == lockerId2;

        private static bool LockerExistsAndInSameDepartment(Locker locker, Guid? departmentId)
            => departmentId is not null && locker.Room.DepartmentId == departmentId;
    }
}