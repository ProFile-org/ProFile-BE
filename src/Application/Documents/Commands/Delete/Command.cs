using Application.Common.Models.Dtos.Physical;
using MediatR;

namespace Application.Documents.Commands.Delete;

public record Command : IRequest<DocumentDto>
{
    public Guid DocumentId { get; init; }
}