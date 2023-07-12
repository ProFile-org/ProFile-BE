using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Digital;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Entries.Queries;

public class GetBinEntryById
{
    public record Query : IRequest<EntryDto>
    {
        public User CurrentUser { get; init; } = null!;
        public Guid EntryId { get; init; }
    }

    public class Handler : IRequestHandler<Query, EntryDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public Handler(IMapper mapper, IApplicationDbContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<EntryDto> Handle(Query request, CancellationToken cancellationToken)
        {
            var entry = await _context.Entries
                .Include(x => x.Owner)
                .Include(x => x.Uploader)
                .FirstOrDefaultAsync(x => x.Id == request.EntryId, cancellationToken);

            if (entry is null)
            {
                throw new KeyNotFoundException("Entry does not exist.");
            }
            
            var entryPath = entry.Path;
            var firstSlashIndex = entryPath.IndexOf("/", StringComparison.Ordinal);
            var binCheck = entryPath.Substring(0, firstSlashIndex);

            if (!binCheck.Contains("_bin"))
            {
                throw new ConflictException("Entry is not in bin.");
            }

            if (!entry.Owner.Username.Equals(request.CurrentUser.Username))
            {
                throw new UnauthorizedAccessException("You do not have permission to view this entry");
            }

            entry.Path = entry.Path[binCheck.Length..];

            return _mapper.Map<EntryDto>(entry);
        }
    }
}