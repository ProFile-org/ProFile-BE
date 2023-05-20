using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using Domain.Entities.Physical;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Documents.Commands.ImportDocument;

public record ImportDocumentCommand : IRequest<DocumentDto>
{
    public string Title { get; set; }
    public string? Description { get; set; }
    public string DocumentType { get; set; }
    public Guid ImporterId { get; set; }
    public Guid FolderId { get; set; }
}

public class ImportDocumentCommandHandler : IRequestHandler<ImportDocumentCommand, DocumentDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public ImportDocumentCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<DocumentDto> Handle(ImportDocumentCommand request, CancellationToken cancellationToken)
    {
        var importer = await _context.Users
            .Include(x => x.Department)
            .FirstOrDefaultAsync(x => x.Id == request.ImporterId, cancellationToken);
        if (importer is null)
        {
            throw new KeyNotFoundException("User does not exist");
        }
        
        var folder = await _context.Folders
            .Include(x => x.Locker)
            .ThenInclude(x => x.Room)
            .FirstOrDefaultAsync(x => x.Id == request.ImporterId, cancellationToken);
        if (folder is null)
        {
            throw new KeyNotFoundException("Folder does not exist");
        }

        var entity = new Document()
        {
            Title = request.Title,
            Description = request.Description,
            DocumentType = request.DocumentType,
            Importer = importer,
            Department = importer.Department,
            Folder = folder
        };

        var result = await _context.Documents.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<DocumentDto>(result.Entity);
    }
} 