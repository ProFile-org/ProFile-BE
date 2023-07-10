using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Documents.Queries;

public abstract class DownloadDocumentFile
{
    public record Query : IRequest<Result>
    {
        public User CurrentUser { get; init; } = null!;
        public Guid DocumentId { get; init; }
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
            var document = await _context.Documents
                .Include(x => x.File)
                .FirstOrDefaultAsync(x => x.Id == request.DocumentId, cancellationToken);

            if (document is null)
            {
                throw new KeyNotFoundException("Document does not exist.");
            }

            if (document.ImporterId != request.CurrentUser.Id)
            {
                throw new UnauthorizedAccessException("User cannot access this resource.");
            }

            if (document.File is null)
            {
                throw new ConflictException("Document does not have a digital file.");
            }

            var result = new Result
            {
                FileName = $"{document.Title}.{document.File.FileExtension}",
                FileType = document.File.FileType,
                FileData = document.File.FileData,
            };

            return result;
        }
    }
}