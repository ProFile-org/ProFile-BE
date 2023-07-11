using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Logging;
using Application.Common.Models.Dtos.Digital;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Digital;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Application.Entries.Commands;

public class RestoreBinEntry
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
        private readonly ILogger<RestoreBinEntry> _logger;
        public Handler(IApplicationDbContext context, IMapper mapper, IDateTimeProvider dateTimeProvider, ILogger<RestoreBinEntry> logger)
        {
            _context = context;
            _mapper = mapper;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
        }

        public async Task<EntryDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var entry = await _context.Entries
                .Include(x => x.Owner)
                .FirstOrDefaultAsync(x => x.Id == request.EntryId, cancellationToken);

            if (entry is null)
            {
                throw new KeyNotFoundException("Entry does not exist.");
            }

            var entryPath = entry.Path;
            var firstSlashIndex = entryPath.IndexOf("/", StringComparison.Ordinal);
            var binCheck = entryPath.Substring(0, firstSlashIndex);

            if (!binCheck.Contains(BinString))
            {
                throw new NotChangedException("Entry is not in bin.");
            }

            if (!entry.Owner.Id.Equals(request.CurrentUser.Id))
            {
                throw new UnauthorizedAccessException("You do not have the permission to restore this entry.");
            }

            var localDateTimeNow = LocalDateTime.FromDateTime(_dateTimeProvider.DateTimeNow);

            var entryCheckPath = entryPath.Replace(binCheck, "");

            var entryCheck = await _context.Entries.FirstOrDefaultAsync(x =>
                (x.Path.Equals("/") ? x.Path + x.Name : x.Path + "/" + x.Name).Equals(entryCheckPath.Equals("/") ? 
                    entryCheckPath + entry.Name : entryCheckPath + "/" + entry.Name), cancellationToken);

            if (entryCheck is not null)
            {
                entryCheck.LastModified = localDateTimeNow;
                entryCheck.LastModifiedBy = request.CurrentUser.Id;
                var dupeResult = _context.Entries.Update(entryCheck);
                _context.Entries.Remove(entry);
                await _context.SaveChangesAsync(cancellationToken);
                return _mapper.Map<EntryDto>(dupeResult.Entity);
            }

            var splitPath = entry.Path.Split("/");
            var currentPath = "/";
            foreach (var node in splitPath)
            {
                if (Array.IndexOf(splitPath, node) == 0)
                {
                    continue;
                }
                
                var entrySearch = await _context.Entries.FirstOrDefaultAsync(x => x.Path.Equals(currentPath) 
                    && x.Name.Equals(node), cancellationToken);
                
                if (entrySearch is not null)
                {
                    continue;
                }

                var parentEntry = new Entry()
                {
                    Name = node,
                    Path = currentPath,
                    Created = localDateTimeNow,
                    CreatedBy = request.CurrentUser.Id,
                    OwnerId = request.CurrentUser.Id,
                    Owner = request.CurrentUser,
                    Uploader = request.CurrentUser,
                };
                await _context.Entries.AddAsync(parentEntry, cancellationToken);
                currentPath = currentPath.Equals("/") ? currentPath + node : currentPath + "/" + node;
            }
            
            if (entry.IsDirectory)
            {
                var path = entry.Path[^1].Equals('/') ? entry.Path + entry.Name : $"{entry.Path}/{entry.Name}";
                var pattern = $"{path}/%";
                var childEntries = _context.Entries.Where(x => 
                    x.Path.Trim().Equals(path)
                    || EF.Functions.Like(x.Path, pattern));

                foreach (var childEntry in childEntries)
                {
                    childEntry.Path = childEntry.Path.Replace(binCheck, "");
                    childEntry.LastModified = localDateTimeNow;
                    childEntry.LastModifiedBy = request.CurrentUser.Id;
                    _context.Entries.Update(childEntry);
                }
            }
            
            entry.Path = entryPath.Replace(binCheck, "");
            entry.LastModified = localDateTimeNow;
            entry.LastModifiedBy = request.CurrentUser.Id;

            var result = _context.Entries.Update(entry);
            await _context.SaveChangesAsync(cancellationToken);
            using (Logging.PushProperties(nameof(Entry), entry.Id, request.CurrentUser.Id))
            {
                _logger.LogRestoreBinEntry(request.CurrentUser.Username, entry.Id.ToString());
            }
            return _mapper.Map<EntryDto>(result.Entity);
        }
    }
}
