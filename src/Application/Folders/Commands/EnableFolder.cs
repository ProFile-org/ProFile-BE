using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Folders.Commands;

public class EnableFolder
{
    public record Command : IRequest<FolderDto>
    {
        public Guid FolderId { get; init; }
    }
    
    public class CommandHandler : IRequestHandler<Command, FolderDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CommandHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<FolderDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var folder = await _context.Folders
                .Include(x => x.Locker)
                .FirstOrDefaultAsync(x => x.Id.Equals(request.FolderId), cancellationToken);

            if (folder is null)
            {
                throw new KeyNotFoundException("Folder does not exist.");
            }

            if (folder.IsAvailable)
            {
                throw new ConflictException("Folder has already been enabled.");
            }

            folder.IsAvailable = true;
            var result = _context.Folders.Update(folder);
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<FolderDto>(result.Entity);
        }
    }
}