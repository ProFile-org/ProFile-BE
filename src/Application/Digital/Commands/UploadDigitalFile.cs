using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Digital;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Digital;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

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
                .Matches("^(/?[a-z_\\-\\s0-9]+)+$").WithMessage("Invalid path format.");
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

        public Handler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        
        public async Task<EntryDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var fileEntry = await _context.Entries.FirstOrDefaultAsync(
                x => x.Name.Trim().Equals(request.Name.Trim())
                && x.Path.Trim().Equals(request.Name.Trim()), cancellationToken);
            
            if (fileEntry is not null)
            {
                throw new ConflictException("File already exists.");
            }
            
            if (request.FileData.Length > 20971520)
            {
                throw new ConflictException("File size must be lower than 20MB");
            }
            
            var fileEntity = new FileEntity()
            {
                FileData = request.FileData.ToArray(),
                FileType = request.FileType,
            };
            
            var entryEntity = new Entry()
            {
                Name = request.Name.Trim(),
                Path = request.Path.Trim(),
                File = fileEntity,
            };
            
            await _context.Files.AddAsync(fileEntity, cancellationToken);
            var result = await _context.Entries.AddAsync(entryEntity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            
            return _mapper.Map<EntryDto>(result.Entity);
        }
    }
}