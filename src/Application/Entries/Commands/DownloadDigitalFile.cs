using Application.Common.Exceptions;
using Application.Common.Extensions.Logging;
using Application.Common.Interfaces;
using Application.Common.Logging;
using Application.Common.Models.Operations;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Entries.Commands;

public class DownloadDigitalFile
{
    public record Command : IRequest<Result>
    {
        public User CurrentUser { get; init; } = null!;
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
        private readonly ILogger<DownloadDigitalFile> _logger;

        public CommandHandler(IApplicationDbContext context, ILogger<DownloadDigitalFile> logger)
        {
            _context = context;
            _logger = logger;
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
                throw new ConflictException("Can not download folder.");
            }
            
            if (entry.OwnerId != request.CurrentUser.Id)
            {
                var permission = await _context.EntryPermissions.FirstOrDefaultAsync(
                    x => x.EntryId == request.EntryId 
                         && x.EmployeeId == request.CurrentUser.Id, cancellationToken);

                if (permission is null ||
                    !permission.AllowedOperations
                        .Split(",")
                        .Contains(EntryOperation.View.ToString()))
                {
                    throw new UnauthorizedAccessException("User cannot access this resource.");
                }
            }

            var content = new MemoryStream(entry.File!.FileData);
            var fileType = entry.File!.FileType;
            var fileId = entry.File!.Id;
            
            using (Logging.PushProperties(nameof(entry), entry.Id, request.CurrentUser.Id))
            {
                _logger.LogDownLoadFile(request.CurrentUser.Username, fileId.ToString());
            }
            
            return new Result()
            {
                Content = content,
                FileType = fileType,
                FileName = $"{entry.Name}.{entry.File.FileExtension}",
            };
        }
    }
}