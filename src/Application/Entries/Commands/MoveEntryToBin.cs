using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Logging;
using Application.Common.Models.Dtos.Digital;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Application.Entries.Commands;

public class MoveEntryToBin
{
    public record Command : IRequest<EntryDto>
    {
        public User CurrentUser { get; init; } = null!;
        public Guid EntryId { get; init; }
    }

    public class Handler : IRequestHandler<Command, EntryDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IDateTimeProvider _dateTimeProvider;
        private const string BinString = "_bin";
        private readonly ILogger<MoveEntryToBin> _logger;

        public Handler(IMapper mapper, IApplicationDbContext context, IDateTimeProvider dateTimeProvider, ILogger<MoveEntryToBin> logger)
        {
            _mapper = mapper;
            _context = context;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
        }

        public async Task<EntryDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var entry = await _context.Entries
                .Include(x=> x.Owner)
                .FirstOrDefaultAsync(x => x.Id == request.EntryId, cancellationToken);

            if (entry is null)
            {
                throw new KeyNotFoundException("Entry does not exist.");
            }

            if (!entry.Owner.Id.Equals(request.CurrentUser.Id))
            {
                throw new UnauthorizedAccessException("You do not have permission to move this entry into bin.");
            }

            var ownerUsername = entry.Owner.Username;

            var localDateTimeNow = LocalDateTime.FromDateTime(_dateTimeProvider.DateTimeNow);

            if (entry.IsDirectory)
            {
                var path = entry.Path.Equals("/") ? entry.Path + entry.Name : $"{entry.Path}/{entry.Name}";
                var pattern = $"{path}/%";
                var childEntries = _context.Entries.Where(x => 
                    x.Path.Trim().Equals(path)
                    || EF.Functions.Like(x.Path, pattern));

                var ids = childEntries.Select(x => x.Id);
                var permissions = _context.EntryPermissions.Where(x => ids.Contains(x.EntryId));
                _context.EntryPermissions.RemoveRange(permissions);

                var c = entry.Path.Length;
                foreach (var childEntry in childEntries)
                {
                    var childBinPath = ownerUsername + BinString;
                    if (!childEntry.Path.Equals("/"))
                    {
                        childBinPath += $"/{childEntry.Path[c..]}";
                    }
                    childEntry.OldPath = childEntry.Path;
                    childEntry.Path = childBinPath;
                    childEntry.LastModified = localDateTimeNow;
                    childEntry.LastModifiedBy = request.CurrentUser.Id;
                    _context.Entries.Update(childEntry);
                }
            }
            
            var binPath = $"{ownerUsername}{BinString}";
            entry.OldPath = entry.Path;
            entry.Path = binPath;
            entry.LastModified = localDateTimeNow;
            entry.LastModifiedBy = request.CurrentUser.Id;

            var entryPermissions = _context.EntryPermissions.Where(x => x.EntryId == entry.Id);
            _context.EntryPermissions.RemoveRange(entryPermissions);

            var result = _context.Entries.Update(entry);
            await _context.SaveChangesAsync(cancellationToken);
            using (Logging.PushProperties(nameof(entry), entry.Id, request.CurrentUser.Id))
            {
                _logger.LogMoveEntryToBin(request.CurrentUser.Username, entry.Id.ToString());
            }
            return _mapper.Map<EntryDto>(result.Entity);
        }
    }
}
