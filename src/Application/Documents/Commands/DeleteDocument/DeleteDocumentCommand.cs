using Application.Common.Models.Dtos.Physical;
using MediatR;

namespace Application.Documents.Commands.DeleteDocument;

public record DeleteDocumentCommand : IRequest<DocumentDto>
{
    public Guid DocumentId { get; init; }
}