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

public class UploadSharedEntry
{
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;
            
            RuleFor(x => x.Name)
                .MaximumLength(256).WithMessage("Name cannot exceed 256 characters.");
        }
    }
    public record Command : IRequest<EntryDto>
    {
        public Guid EntryId { get; init; }
        public User CurrentUser { get; init; } = null!;
        public string Name { get; init; } = null!;
        public bool IsDirectory { get; init; }
        public MemoryStream? FileData { get; init; }
        public string? FileType { get; init; }
        public string? FileExtension { get; init; }
    }
    
    public class CommandHandler : IRequestHandler<Command, EntryDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger<UploadSharedEntry> _logger;
        public CommandHandler(IApplicationDbContext context, IMapper mapper, IDateTimeProvider dateTimeProvider, ILogger<UploadSharedEntry> logger)
        {
            _context = context;
            _mapper = mapper;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
        }

        public async Task<EntryDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var permissions = _context.EntryPermissions
                .Include(x => x.Entry)
                .ThenInclude(y => y.Uploader)
                .Include(x => x.Entry)
                .ThenInclude(x => x.Owner)
                .Where(x => x.EmployeeId == request.CurrentUser.Id 
                            && x.AllowedOperations.Contains(EntryOperation.Upload.ToString()));

            var rootEntry = await permissions
                .Select(x => x.Entry)
                .FirstOrDefaultAsync(x => x.Id == request.EntryId, cancellationToken);

            if (rootEntry is null)
            {
                throw new KeyNotFoundException("Entry does not exist.");
            }

            if (!rootEntry.IsDirectory)
            {
                throw new ConflictException("This is a file.");
            }
            
            var localDateTimeNow = LocalDateTime.FromDateTime(_dateTimeProvider.DateTimeNow);
            var entryPath = rootEntry.Path + "/" + request.Name.Trim();
            
            var entity = new Entry()
            {
                Name = request.Name.Trim(),
                Path = entryPath,
                CreatedBy = request.CurrentUser.Id,
                Uploader = request.CurrentUser,
                Created = localDateTimeNow,
                Owner = rootEntry.Owner,
                OwnerId = rootEntry.Owner.Id,
            };

            if (request.IsDirectory)
            {
                var entry = await _context.Entries
                    .FirstOrDefaultAsync(
                        x => x.Name.Trim().Equals(request.Name.Trim()) 
                             && x.Path.Equals(entryPath) 
                             && x.FileId == null,cancellationToken);
                
                if (entry is not null)
                {
                    throw new ConflictException("Directory already exists.");
                }
            }
            else
            {
                if (request.FileData!.Length > 20971520)
                {
                    throw new ConflictException("File size must be lower than 20MB");
                }
                
                var lastDotIndex = request.Name.LastIndexOf(".", StringComparison.Ordinal);
                var fileExtension = request.Name.Substring(lastDotIndex + 1, request.Name.Length - lastDotIndex - 1);
            
                var fileEntity = new FileEntity()
                {
                    FileData = request.FileData.ToArray(),
                    FileType = request.FileType!,
                    FileExtension = request.FileExtension,
                };
                
                await _context.Files.AddAsync(fileEntity, cancellationToken);

                entity.FileId = fileEntity.Id;
                entity.File = fileEntity;
            }
            
            var result = await _context.Entries.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            using (Logging.PushProperties(nameof(Entry), entity.Id, request.CurrentUser.Id))
            {
                _logger.LogUploadSharedEntry(request.CurrentUser.Id.ToString(), entity.Id.ToString());
            }
            return _mapper.Map<EntryDto>(result.Entity);
        }
    }
}