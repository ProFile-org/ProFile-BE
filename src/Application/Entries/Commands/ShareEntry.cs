using System.Configuration;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Logging;
using Application.Common.Models.Dtos.Digital;
using Application.Common.Models.Operations;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Digital;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Application.Entries.Commands;

public class ShareEntry
{
    public record Command : IRequest<EntryPermissionDto>
    {
        public User CurrentUser { get; init; } = null!;
        public Guid EntryId { get; init; }
        public Guid UserId { get; init; }
        public DateTime? ExpiryDate { get; init; }
        public bool CanView { get; init; }
        public bool CanEdit { get; init; }
    }
    
    public class CommandHandler : IRequestHandler<Command, EntryPermissionDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger<ShareEntry> _logger;

        public CommandHandler(IApplicationDbContext applicationDbContext, IMapper mapper, IDateTimeProvider dateTimeProvider, ILogger<ShareEntry> logger)
        {
            _context = applicationDbContext;
            _mapper = mapper;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
        }
        
        public async Task<EntryPermissionDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var entry = await _context.Entries
                .FirstOrDefaultAsync(x => x.Id == request.EntryId, cancellationToken);
            
            if (entry is null)
            {
                throw new KeyNotFoundException("Entry does not exist.");
            }

            var canChangeEntryPermission = _context.EntryPermissions.Any(x =>
                x.EntryId == request.EntryId
                && x.EmployeeId == request.CurrentUser.Id
                && x.AllowedOperations.Contains(EntryOperation.Edit.ToString()));
            
            if (entry.OwnerId != request.CurrentUser.Id && !canChangeEntryPermission)
            {
                throw new UnauthorizedAccessException("You are not allow to change permission of this entry.");
            }
            
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == request.UserId, cancellationToken);

            if (user is null)
            {
                throw new KeyNotFoundException("User does not exist.");
            }

            if (user.Id == entry.OwnerId)
            {
                throw new ConflictException("You can not modify owner's permission.");
            }

            if (request.ExpiryDate < _dateTimeProvider.DateTimeNow)
            {
                throw new ConflictException("Expiry date cannot be in the past.");
            }

            var allowOperations = GenerateAllowOperations(request);


            var isShareRoot = !await _context.EntryPermissions
                .AnyAsync(x => (x.Entry.Path == "/" ? x.Entry.Path + x.Entry.Name : x.Entry.Path + "/" + x.Entry.Name) == entry.Path
                               && x.EmployeeId == request.UserId
                               && x.Entry.FileId == null, cancellationToken);
            
            await GrantOrRevokePermission(entry, user, allowOperations, request.ExpiryDate, isShareRoot, cancellationToken);

            if (entry.IsDirectory)
            {
                // Update permissions for child Entries
                var path = entry.Path.Equals("/") ? entry.Path + entry.Name : $"{entry.Path}/{entry.Name}";
                var pattern = $"{path}/%";
                var childEntries = _context.Entries
                    .Where(x => (x.Path.Equals(path) || EF.Functions.Like(x.Path, pattern))
                                && x.OwnerId == entry.OwnerId)
                    .ToList();
                foreach (var childEntry in childEntries)
                {
                    var childAllowOperations = GenerateAllowOperations(request);
                    await GrantOrRevokePermission(childEntry, user, childAllowOperations, request.ExpiryDate, false, cancellationToken);
                }
            }
            await _context.SaveChangesAsync(cancellationToken);
            using (Logging.PushProperties(nameof(Entry), entry.Id, request.CurrentUser.Id))
            {
                _logger.LogShareEntry(request.CurrentUser.Username, entry.Id.ToString(), user.Username);
            }
            var result = await _context.EntryPermissions.FirstOrDefaultAsync(x =>
                    x.EntryId == request.EntryId
                    && x.EmployeeId == request.UserId, cancellationToken);
            return _mapper.Map<EntryPermissionDto>(result);
        }

        private async Task GrantOrRevokePermission(
            Entry entry,
            User user,
            string allowOperations,
            DateTime? expiryDate,
            bool isSharedRoot,
            CancellationToken cancellationToken)
        {
            var existedPermission = await _context.EntryPermissions.FirstOrDefaultAsync(x =>
                    x.EntryId == entry.Id
                    && x.EmployeeId == user.Id, cancellationToken);
            
            if (existedPermission is null)
            {
                if (string.IsNullOrEmpty(allowOperations)) return;
                var entryPermission = new EntryPermission
                {
                    EmployeeId = user.Id,
                    EntryId = entry.Id,
                    AllowedOperations = allowOperations,
                    ExpiryDateTime = expiryDate is null ? null : LocalDateTime.FromDateTime(expiryDate.Value),
                    IsSharedRoot = isSharedRoot,
                    Employee = user,
                    Entry = entry,
                };
                await _context.EntryPermissions.AddAsync(entryPermission, cancellationToken);
            }
            else if (string.IsNullOrEmpty(allowOperations))
            {
                _context.EntryPermissions.Remove(existedPermission);
            }
            else
            {
                existedPermission.AllowedOperations = allowOperations;
                existedPermission.ExpiryDateTime = expiryDate is null ? null : LocalDateTime.FromDateTime(expiryDate.Value);
                _context.EntryPermissions.Update(existedPermission);
            }
        }

        private static string GenerateAllowOperations(Command request)
        {
            var allowOperations = new CommaDelimitedStringCollection();

            if (request.CanView)
            {
                allowOperations.Add(EntryOperation.View.ToString());
            }
            else
            {
                return string.Empty;
            }
            
            if (request is { CanView: true, CanEdit: true })
            {
                allowOperations.Add(EntryOperation.Edit.ToString());
            }

            return allowOperations.ToString();
        }
    }
}