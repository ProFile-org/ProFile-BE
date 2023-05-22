using Application.Common.Models.Dtos.Physical;
using MediatR;

namespace Application.Documents.Commands.UpdateDocument;

public record UpdateDocumentCommand : IRequest<DocumentDto>
{
    public Guid Id { get; set; }
    public string? Title { get; set; } 
    public string? Description { get; set; }
    public string? DocumentType { get; set; }
    public Guid? Department { get; set; }
    public Guid? Importer { get; set; }
    public Guid? Folder { get; set; }
}