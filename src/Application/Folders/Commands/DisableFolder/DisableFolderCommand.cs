using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Folders.Commands.DisableFolder;

public record DisableFolderCommand : IRequest<FolderDto>
{
    public Guid FolderId { get; init; }
}

public class DisableFolderCommandHandler : IRequestHandler<DisableFolderCommand, FolderDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public DisableFolderCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<FolderDto> Handle(DisableFolderCommand request, CancellationToken cancellationToken)
    {
        var folder = await _context.Folders
            .Include(f => f.Locker)
            .FirstOrDefaultAsync(f => f.Id.Equals(request.FolderId), cancellationToken);

        if (folder is null)
        {
            throw new KeyNotFoundException("Folder does not exist.");
        }

        if (!folder.IsAvailable)
        {
            throw new InvalidOperationException("Folder already disabled.");
        }
        
        if (folder.NumberOfDocuments > 0)
        {
            throw new InvalidOperationException("Folder cannot be disabled because it contains documents.");
        }

        folder.IsAvailable = false;
        _context.Folders.Update(folder);
        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<FolderDto>(folder);
    }
}