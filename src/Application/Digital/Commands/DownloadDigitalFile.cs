using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models.Dtos;
using Application.Common.Models.Operations;
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
            
            if (entry.OwnerId != request.CurrentUserId)
            {
                var permission = await _context.EntryPermissions.FirstOrDefaultAsync(
                    x => x.EntryId == request.EntryId 
                         && x.EmployeeId == request.CurrentUserId, cancellationToken);

                if (permission is null ||
                    !permission.AllowedOperations
                        .Split(",")
                        .Contains(EntryOperation.Download.ToString()))
                {
                    throw new UnauthorizedAccessException("User cannot access this resource.");
                }
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