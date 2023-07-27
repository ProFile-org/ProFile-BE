using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Logging;
using Application.Common.Models.Dtos.Digital;
using Application.Common.Models.Operations;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Digital;
using Domain.Entities.Physical;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Application.Entries.Commands;

public class CreateSharedEntry
{
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;
            
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Entry's name is required.")
                .Matches("^[\\p{L}A-Za-z_.\\s\\-0-9]*$").WithMessage("Invalid name format.")
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
        private readonly ILogger<CreateSharedEntry> _logger;
        public CommandHandler(IApplicationDbContext context, IMapper mapper, IDateTimeProvider dateTimeProvider, ILogger<CreateSharedEntry> logger)
        {
            _context = context;
            _mapper = mapper;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
        }

        public async Task<EntryDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var permission = await _context.EntryPermissions
                .Include(x => x.Entry)
                .ThenInclude(y => y.Uploader)
                .Include(x => x.Entry)
                .ThenInclude(x => x.Owner)
                .FirstOrDefaultAsync(x => x.EmployeeId == request.CurrentUser.Id &&
                                          x.EntryId == request.EntryId, cancellationToken);
            if (permission is null)
            {
                throw new UnauthorizedAccessException("User is not allowed to create an entry.");
            }

            if (!permission.Entry.IsDirectory)
            {
                throw new ConflictException("This is a file.");
            }

            var localDateTimeNow = LocalDateTime.FromDateTime(_dateTimeProvider.DateTimeNow);
            var entryPath = permission.Entry.Path.Equals("/") ? permission.Entry.Path + permission.Entry.Name : $"{permission.Entry.Path}/{request.Name.Trim()}";
            
            var entity = new Entry()
            {
                Name = request.Name.Trim(),
                Path = entryPath,
                CreatedBy = request.CurrentUser.Id,
                Uploader = request.CurrentUser,
                Created = localDateTimeNow,
                Owner = permission.Entry.Owner,
                OwnerId = permission.Entry.Owner.Id,
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

            var permissionEntity = new EntryPermission()
            {
                EntryId = result.Entity.Id,
                Entry = result.Entity,
                EmployeeId = request.CurrentUser.Id,
                Employee = request.CurrentUser,
                ExpiryDateTime = null,
                AllowedOperations = $"{EntryOperation.View.ToString()},{EntryOperation.Edit.ToString()}"
            };
            
            await _context.EntryPermissions.AddAsync(permissionEntity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            using (Logging.PushProperties(nameof(Entry), entity.Id, request.CurrentUser.Id))
            {
                _logger.LogCreateSharedEntry(request.CurrentUser.Username, entity.Id.ToString());
            }
            return _mapper.Map<EntryDto>(result.Entity);
        }
    }
}