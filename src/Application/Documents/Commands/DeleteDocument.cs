using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Documents.Commands;

public class DeleteDocument
{
    public record Command : IRequest<DocumentDto>
    {
        public Guid DocumentId { get; init; }
    }
    
    public class CommandHandler : IRequestHandler<Command, DocumentDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CommandHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<DocumentDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var document = await _context.Documents
                .Include( x => x.Folder)
                .FirstOrDefaultAsync(x => x.Id.Equals(request.DocumentId), cancellationToken);

            if (document is null)
            {
                throw new KeyNotFoundException("Document does not exist.");
            }

            var folder = document.Folder;
            var result = _context.Documents.Remove(document);

            if (folder is not null)
            {
                folder.NumberOfDocuments -= 1;
                _context.Folders.Update(folder);
            }
            
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<DocumentDto>(result.Entity);
        }
    }
}