using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Digital;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Application.Digital.Commands;

public class UpdateEntry
{
    public record Command : IRequest<EntryDto>
    {
        public Guid CurrentUserId { get; init; }
        public Guid EntryId { get; init; }
        public string Name { get; init; } = null!;
    }
    
    public class CommandHandler : IRequestHandler<Command, EntryDto>
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

        public async Task<EntryDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var entry = await _context.Entries
                .FirstOrDefaultAsync(x => x.Id == request.EntryId, cancellationToken);

            if (entry is null)
            {
                throw new KeyNotFoundException("Entry does not exist.");
            }

            var nameExisted = await _context.Entries
                .AnyAsync(x => x.Id != entry.Id 
                               && x.Path.Equals(entry.Path)
                               && x.Name.Equals(request.Name), cancellationToken);

            if (nameExisted)
            {
                throw new ConflictException("Name has already exist.");
            }

            var localDateTimeNow = LocalDateTime.FromDateTime(_dateTimeProvider.DateTimeNow);
            
            entry.Name = request.Name;
            entry.LastModified = localDateTimeNow;
            entry.LastModifiedBy = request.CurrentUserId;

            var result = _context.Entries.Update(entry);
            await _context.SaveChangesAsync(cancellationToken);

            return _mapper.Map<EntryDto>(result.Entity);
        }
    }
}