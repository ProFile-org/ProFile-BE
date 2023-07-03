using System.Configuration;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Digital;
using Application.Common.Models.Operations;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Digital;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Application.Digital.Commands;

public class ShareEntry
{
    public record Command : IRequest<EntryPermissionDto>
    {
        public User CurrentUser { get; init; } = null!;
        public Guid EntryId { get; init; }
        public Guid UserId { get; init; }
        public DateTime ExpiryDate { get; init; }
        public bool CanView { get; init; }
        public bool CanUpload { get; init; }
        public bool CanDownload { get; init; }
        public bool CanChangePermission { get; init; }
    }
    
    public class CommandHandler : IRequestHandler<Command, EntryPermissionDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IDateTimeProvider _dateTimeProvider;

        public CommandHandler(IApplicationDbContext applicationDbContext, IMapper mapper, IDateTimeProvider dateTimeProvider)
        {
            _context = applicationDbContext;
            _mapper = mapper;
            _dateTimeProvider = dateTimeProvider;
        }
        
        public async Task<EntryPermissionDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var entry = await _context.Entries
                .Include(x => x.Uploader)
                .Include(x => x.Owner)
                .FirstOrDefaultAsync(x => x.Id == request.EntryId, cancellationToken);
            
            if (entry is null)
            {
                throw new KeyNotFoundException("Entry does not exist.");
            }

            var hasModifyPermission = _context.EntryPermissions.Any(x =>
                x.EntryId == request.EntryId
                && x.EmployeeId == request.CurrentUser.Id
                && x.AllowedOperations.Contains(EntryOperation.ChangePermission.ToString()));
            
            if (entry.Owner!.Id != request.CurrentUser.Id && !hasModifyPermission)
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

            var allowOperations = GenerateAllowOperations(request, entry.IsDirectory);
            
            await GrantPermission(entry, user, allowOperations, request.ExpiryDate, cancellationToken);

            if (entry.IsDirectory)
            {
                // Update permissions for child Entries
                var path = entry.Path.Equals("/") ? (entry.Path + entry.Name) : (entry.Path + "/" + entry.Name);
                var childEntries = _context.Entries
                    .Include(x => x.Uploader)
                    .Include(x => x.Owner)
                    .Where(x => x.Id != entry.Id 
                                && x.Path.StartsWith(path)
                                && x.OwnerId == entry.OwnerId)
                    .ToList();
                foreach (var childEntry in childEntries)
                {
                    var childAllowOperations = GenerateAllowOperations(request, childEntry.IsDirectory);
                    await GrantPermission(childEntry, user, childAllowOperations, request.ExpiryDate, cancellationToken);
                }
            }

            var result = await _context.EntryPermissions.FirstOrDefaultAsync(x =>
                    x.EntryId == request.EntryId
                    && x.EmployeeId == request.UserId
                , cancellationToken);
            
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<EntryPermissionDto>(result);
        }

        private async Task GrantPermission(
            Entry entry,
            User user,
            string allowOperations,
            DateTime expiryDate,
            CancellationToken cancellationToken)
        {
            var existedPermission = await _context.EntryPermissions.FirstOrDefaultAsync(x =>
                    x.EntryId == entry.Id
                    && x.EmployeeId == user.Id
                , cancellationToken);
            
            if (existedPermission is null)
            {
                var entryPermission = new EntryPermission
                {
                    EmployeeId = user.Id,
                    EntryId = entry.Id,
                    AllowedOperations = allowOperations,
                    ExpiryDateTime = LocalDateTime.FromDateTime(expiryDate),
                    Employee = user,
                    Entry = entry,
                };
                await _context.EntryPermissions.AddAsync(entryPermission, cancellationToken);
            }
            else
            {
                existedPermission.AllowedOperations = allowOperations;
                existedPermission.ExpiryDateTime = LocalDateTime.FromDateTime(expiryDate);
                _context.EntryPermissions.Update(existedPermission);
            }
            await _context.SaveChangesAsync(cancellationToken);
        }

        private static string GenerateAllowOperations(Command request, bool isDirectory)
        {
            if (request is { CanView: false, CanDownload: false, CanUpload: false, CanChangePermission: false })
            {
                return string.Empty;
            }
            
            var allowOperations = new CommaDelimitedStringCollection();

            if (request.CanView)
            {
                allowOperations.Add(EntryOperation.View.ToString());
            }
            
            if (request is { CanView: true, CanUpload: true } && !isDirectory)
            {
                allowOperations.Add(EntryOperation.Upload.ToString());
            }
            
            if (request is { CanView: true, CanDownload: true } && !isDirectory)
            {
                allowOperations.Add(EntryOperation.Download.ToString());
            }
            
            if (request is { CanView: true, CanChangePermission: true })
            {
                allowOperations.Add(EntryOperation.ChangePermission.ToString());
            }

            return allowOperations.ToString();
        }
    }
}