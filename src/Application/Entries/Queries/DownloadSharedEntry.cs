using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models.Operations;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Entries.Queries;

public class DownloadSharedEntry
{
    public record Query : IRequest<Result>
    {
        public User CurrentUser { get; init; } = null!;
        public Guid EntryId { get; init; }
    }

    public class Result
    {
        public string FileName { get; set; } = null!;
        public string FileType { get; set; } = null!;
        public byte[] FileData { get; set; } = null!;
    }

    public class QueryHandler : IRequestHandler<Query, Result>
    {
        private readonly IApplicationDbContext _context;
        public QueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(Query request, CancellationToken cancellationToken)
        {
            var entry = await _context.Entries.FirstOrDefaultAsync(x => x.Id == request.EntryId, cancellationToken);

            if (entry is null)
            {
                throw new KeyNotFoundException("Entry does not exist.");
            }

            if (entry.IsDirectory)
            {
                throw new ConflictException("Entry cannot be downloaded.");
            }

            if (entry.OwnerId != request.CurrentUser.Id)
            {
                var permission = await _context.EntryPermissions.FirstOrDefaultAsync(
                    x => x.EntryId == request.EntryId && x.EmployeeId == request.CurrentUser.Id, cancellationToken);

                if (permission is null ||
                    !permission.AllowedOperations
                        .Split(",")
                        .Contains(EntryOperation.View.ToString()))
                {
                    throw new UnauthorizedAccessException("User cannot access this resource.");
                }
            }
            
            var result = new Result
            {
                FileName = $"{entry.Name}.{entry.File!.FileExtension}",
                FileType = entry.File.FileType,
                FileData = entry.File.FileData,
            };

            return result;
        }
    }
}