using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Digital;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Application.Digital.Commands;

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

        public Handler(IMapper mapper, IApplicationDbContext context, IDateTimeProvider dateTimeProvider)
        {
            _mapper = mapper;
            _context = context;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<EntryDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var entry = await _context.Entries
                .Include(x=> x.Owner)
                .FirstOrDefaultAsync(x => x.Id == request.EntryId, cancellationToken);

            if (entry is null)
            {
                throw new KeyNotFoundException("Entry does not exist");
            }
            
            var path = entry.Path;
            var firstSlashIndex = path.IndexOf("/", StringComparison.Ordinal);
            var binCheck = path.Substring(0, firstSlashIndex);

            if (binCheck.Contains(BinString))
            {
                throw new ConflictException("Entry is already in bin");
            }
            
            var ownerUsername = entry.Owner.Username;

            var binPath = ownerUsername + BinString + path;
            
            var localDateTimeNow = LocalDateTime.FromDateTime(_dateTimeProvider.DateTimeNow);

            entry.Path = binPath;
            entry.LastModified = localDateTimeNow;
            entry.LastModifiedBy = request.CurrentUser.Id;
            
            var result = _context.Entries.Update(entry);
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<EntryDto>(result.Entity);
        }
    }
}