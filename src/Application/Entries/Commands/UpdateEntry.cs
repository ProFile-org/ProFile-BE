using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Logging;
using Application.Common.Models.Dtos.Digital;
using Application.Common.Models.Operations;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Digital;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Application.Entries.Commands;

public class UpdateEntry
{
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;
            
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Entry's name is required.")
                .MaximumLength(256).WithMessage("Name cannot exceed 256 characters.");
        }
    }
    
    public record Command : IRequest<EntryDto>
    {
        public User CurrentUser { get; init; } = null!;
        public Guid EntryId { get; init; }
        public string Name { get; init; } = null!;
    }
    
    public class CommandHandler : IRequestHandler<Command, EntryDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger<UpdateEntry> _logger;

        public CommandHandler(IApplicationDbContext context, IMapper mapper, IDateTimeProvider dateTimeProvider, ILogger<UpdateEntry> logger)
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
            
            if (entry.OwnerId != request.CurrentUser.Id)
            {
                var permission = await _context.EntryPermissions.FirstOrDefaultAsync(
                    x => x.EntryId == request.EntryId
                         && x.EmployeeId == request.CurrentUser.Id, cancellationToken);

                if (permission is null ||
                    !permission.AllowedOperations
                        .Split(",")
                        .Contains(EntryOperation.Edit.ToString()))
                {
                    throw new UnauthorizedAccessException("User cannot access this resource.");
                }
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
            entry.LastModifiedBy = request.CurrentUser.Id;

            var result = _context.Entries.Update(entry);
            await _context.SaveChangesAsync(cancellationToken);
            using (Logging.PushProperties(nameof(Entry), entry.Id, request.CurrentUser.Id))
            {
                _logger.LogUpdateEntry(request.CurrentUser.Username, entry.Id.ToString());
            }
            return _mapper.Map<EntryDto>(result.Entity);
        }
    }
}