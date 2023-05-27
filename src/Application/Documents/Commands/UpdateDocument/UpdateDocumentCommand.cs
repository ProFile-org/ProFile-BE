using Application.Common.Models.Dtos.Physical;
using MediatR;

namespace Application.Documents.Commands.UpdateDocument;

public record UpdateDocumentCommand : IRequest<DocumentDto>
{
    public Guid DocumentId { get; init; }
    public string Title { get; init; } = null!; 
    public string? Description { get; init; }
    public string DocumentType { get; init; } = null!; 
}