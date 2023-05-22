using Application.Common.Models.Dtos.Physical;
using MediatR;

namespace Application.Documents.Queries.GetDocumentById;

public record GetDocumentByIdQuery : IRequest<DocumentDto>
{
    public Guid Id { get; set; } 
}