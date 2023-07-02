using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models.Dtos;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Digital.Commands;

public class DownloadDigitalFile
{
    public record Command : IRequest<Result>
    {
        public Guid EntryId { get; init; }
    }

    public class Result : BaseDto
    {
        public MemoryStream Content { get; set; } = null!;
        public string FileType { get; set; } = null!;
        public string FileName { get; set; } = null!;
    }
    
    
    public class CommandHandler : IRequestHandler<Command, Result>
    {
        private readonly IApplicationDbContext _context;

        public CommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var entry = await _context.Entries
                .Include(x => x.File)
                .FirstOrDefaultAsync(x => x.Id == request.EntryId, cancellationToken);

            if (entry is null)
            {
                throw new KeyNotFoundException("Entry does not exist.");
            }

            if (entry.IsDirectory)
            {
                throw new ConflictException("Can not delete folder.");
            }

            var content = new MemoryStream(entry.File!.FileData);
            var fileType = entry.File!.FileType;
            var fileExtension = entry.File!.FileExtension;
            
            return new Result()
            {
                Id = entry.File.Id,
                Content = content,
                FileType = fileType,
                FileName = $"{entry.Name}.{fileExtension}",
            };
        }
    }
}