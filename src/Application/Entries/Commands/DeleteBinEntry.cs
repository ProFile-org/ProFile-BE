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

namespace Application.Entries.Commands;

public class DeleteBinEntry
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
        private readonly ILogger<DeleteBinEntry> _logger;

        public Handler(IMapper mapper, IApplicationDbContext context, IDateTimeProvider dateTimeProvider, ILogger<DeleteBinEntry> logger)
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

            var entryPath = entry.Path;
            var firstSlashIndex = entryPath.IndexOf("/", StringComparison.Ordinal);
            var binCheck = entryPath.Substring(0, firstSlashIndex);

            if (!binCheck.Contains(BinString))
            {
                throw new NotChangedException("Entry is not in bin.");
            }
            
            if (!entry.Owner.Username.Equals(request.CurrentUser.Username))
            {
                throw new UnauthorizedAccessException("You do not have the permission to delete this entry.");
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
                    _context.Entries.Remove(childEntry);
                }
            }

            var result = _context.Entries.Remove(entry);
            await _context.SaveChangesAsync(cancellationToken);
            using (Logging.PushProperties(nameof(Entry), entry.Id, request.CurrentUser.Id))
            {
                _logger.LogDeleteBinEntry(request.CurrentUser.Username, entry.Id.ToString());
            }
            return _mapper.Map<EntryDto>(result.Entity);
        }
    }
}
