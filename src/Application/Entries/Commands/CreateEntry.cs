using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Logging;
using Application.Common.Models.Dtos.Digital;
using Application.Helpers;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Digital;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Application.Entries.Commands;

public class CreateEntry {
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;
            
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Entry's name is required.")
                .MaximumLength(256).WithMessage("Name cannot exceed 256 characters.");

            RuleFor(x => x.Path)
                .NotEmpty().WithMessage("Entry's path is required.")
                .Matches("^(/(?!/)[\\p{L}A-Za-z_.\\s\\-0-9]*)+(?<!/)$|^/$").WithMessage("Invalid path format.");
        }
    }
    
    public record Command : IRequest<EntryDto>
    {
        public User CurrentUser { get; init; } = null!;
        public string Name { get; init; } = null!;
        public string Path { get; init; } = null!;
        public bool IsDirectory { get; init; }
        public MemoryStream? FileData { get; init; }
        public string? FileType { get; init; }
        public string? FileExtension { get; init; }
    }
    
    public class Handler : IRequestHandler<Command, EntryDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper; 
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger<CreateEntry> _logger;

        public Handler(IApplicationDbContext context, IMapper mapper, IDateTimeProvider dateTimeProvider, ILogger<CreateEntry> logger)
        {
            _context = context;
            _mapper = mapper;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
        }
        
        public async Task<EntryDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var baseDirectoryExists = await _context.Entries.AnyAsync(
                x => request.Path.Trim().ToLower()
                         .Equals((x.Path.Equals("/") ? (x.Path + x.Name) : (x.Path + "/" + x.Name)).ToLower())
                     && x.FileId == null, cancellationToken);

            if (!request.Path.Equals("/") && !baseDirectoryExists)
            {
                throw new ConflictException("Base directory does not exist.");
            }

            var localDateTimeNow = LocalDateTime.FromDateTime(_dateTimeProvider.DateTimeNow);

            var entryEntity = new Entry()
            {
                Name = request.Name.Trim(),
                Path = request.Path.Trim(),
                CreatedBy = request.CurrentUser.Id,
                Uploader = request.CurrentUser,
                Created = localDateTimeNow,
                Owner = request.CurrentUser,
                OwnerId = request.CurrentUser.Id,
                SizeInBytes = null
            };
            
            if (request.IsDirectory)
            {
                var entry = await _context.Entries.FirstOrDefaultAsync(
                    x => x.Name.Trim().Equals(request.Name.Trim())
                         && x.Path.Trim().Equals(request.Path.Trim())
                    && x.FileId == null, cancellationToken);
            
                if (entry is not null)
                {
                    throw new ConflictException("Directory already exists.");
                }
            }
            else
            {
                // Make this dynamic
                if (request.FileData!.Length > FileUtil.ToByteFromMb(20))
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
                entryEntity.FileId = fileEntity.Id;
                entryEntity.File = fileEntity;
                entryEntity.SizeInBytes = request.FileData!.Length;
                
                await _context.Files.AddAsync(fileEntity, cancellationToken);
            }
            
            var result = await _context.Entries.AddAsync(entryEntity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            using (Logging.PushProperties(nameof(Entry), entryEntity.Id, request.CurrentUser.Id))
            {
                _logger.LogCreateEntry(request.CurrentUser.Username, entryEntity.Id.ToString());
            }
            return _mapper.Map<EntryDto>(result.Entity);
        }
    }
}