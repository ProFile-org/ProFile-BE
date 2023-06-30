using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Digital;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Digital;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Application.Digital.Commands;

public class UploadDigitalFile {
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;
            RuleFor(x => x.Name)
                .MaximumLength(256).WithMessage("Name cannot exceed 256 characters.");
            RuleFor(x => x.Path)
                .NotEmpty().WithMessage("File's path is required.")
                .Matches("^(/(?!/)[a-z_.\\-0-9]*)+(?<!/)$|^/$").WithMessage("Invalid path format.");
        }
    }
    
    public record Command : IRequest<EntryDto>
    {
        public User CurrentUser { get; init; } = null!;
        public string Name { get; init; } = null!;
        public string Path { get; init; } = null!;
        public MemoryStream FileData { get; init; } = null!;
        public string FileType { get; init; } = null!;
    }
    
    public class Handler : IRequestHandler<Command, EntryDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper; 
        private readonly IDateTimeProvider _dateTimeProvider;

        public Handler(IApplicationDbContext context, IMapper mapper, IDateTimeProvider dateTimeProvider)
        {
            _context = context;
            _mapper = mapper;
            _dateTimeProvider = dateTimeProvider;
        }
        
        public async Task<EntryDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var fileEntry = await _context.Entries.FirstOrDefaultAsync(
                x => x.Name.Trim().Equals(request.Name.Trim())
                && x.Path.Trim().Equals(request.Path.Trim()), cancellationToken);
            
            if (fileEntry is not null)
            {
                throw new ConflictException("File already exists.");
            }

            var directoryExists = await _context.Entries.AnyAsync(
                x => request.Path.Trim().ToLower().Equals((x.Path + x.Name).ToLower())
                     && x.FileId == null, cancellationToken);

            if (!request.Path.Equals("/") && !directoryExists)
            {
                throw new ConflictException("Directory does not exist.");
            }
            
            // Make this dynamic
            if (request.FileData.Length > 20971520)
            {
                throw new ConflictException("File size must be lower than 20MB");
            }
            
            var localDateTimeNow = LocalDateTime.FromDateTime(_dateTimeProvider.DateTimeNow);

            var lastDotIndex = request.Name.LastIndexOf(".", StringComparison.Ordinal);
            var fileExtension = request.Name.Substring(lastDotIndex + 1, request.Name.Length - lastDotIndex - 1);
            
            var fileEntity = new FileEntity()
            {
                FileData = request.FileData.ToArray(),
                FileType = request.FileType,
                FileExtension = fileExtension,
            };
            
            var entryEntity = new Entry()
            {
                Name = request.Name.Trim(),
                Path = request.Path.Trim(),
                File = fileEntity,
                CreatedBy = request.CurrentUser.Id,
                Uploader = request.CurrentUser,
                Created = localDateTimeNow,
            };
            
            await _context.Files.AddAsync(fileEntity, cancellationToken);
            var result = await _context.Entries.AddAsync(entryEntity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            
            return _mapper.Map<EntryDto>(result.Entity);
        }
    }
}