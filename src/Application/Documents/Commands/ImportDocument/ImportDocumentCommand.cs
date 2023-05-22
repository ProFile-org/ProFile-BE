using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using Domain.Entities.Physical;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Documents.Commands.ImportDocument;

public record ImportDocumentCommand : IRequest<DocumentDto>
{
    public string Title { get; init; }
    public string? Description { get; init; }
    public string DocumentType { get; init; }
    public Guid ImporterId { get; init; }
    public Guid FolderId { get; init; }
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

        var document = _context.Documents.FirstOrDefault(x => 
            x.Title.Equals(request.Title) 
            && x.Importer != null 
            && x.Importer.Id == request.ImporterId);
        if (document is not null)
        {
            throw new ConflictException($"Document title already exists for user {importer.LastName}");
        }
        
        var folder = await _context.Folders
            .FirstOrDefaultAsync(x => x.Id == request.FolderId, cancellationToken);
        if (folder is null)
        {
            throw new KeyNotFoundException("Folder does not exist");
        }

        var entity = new Document()
        {
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            DocumentType = request.DocumentType.Trim(),
            Importer = importer,
            Department = importer.Department,
            Folder = folder
        };

        var result = await _context.Documents.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<DocumentDto>(result.Entity);
    }
} 