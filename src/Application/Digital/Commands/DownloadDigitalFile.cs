using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models.Dtos;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Digital.Commands;

public class DownloadDigitalFile
{
    public record Command : IRequest<Result>
    {
        public Guid CurrentUserId { get; init;}
        public Guid EntryId { get; init; }
    }

    public class Result
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
                .Include(x => x.Owner)
                .FirstOrDefaultAsync(x => x.Id == request.EntryId, cancellationToken);

            if (entry is null)
            {
                throw new KeyNotFoundException("Entry does not exist.");
            }
            
            if (entry.IsDirectory)
            {
                throw new ConflictException("Can not download folder.");
            }
            
            
            if (entry.Owner.Id != request.CurrentUserId)
            {
                throw new UnauthorizedAccessException("Can not download file that does not belong to you.");
            }

            var content = new MemoryStream(entry.File!.FileData);
            var fileType = entry.File!.FileType;
            var fileExtension = entry.File!.FileExtension;
            
            return new Result()
            {
                Content = content,
                FileType = fileType,
                FileName = $"{entry.Name}.{fileExtension}",
            };
        }
    }
}