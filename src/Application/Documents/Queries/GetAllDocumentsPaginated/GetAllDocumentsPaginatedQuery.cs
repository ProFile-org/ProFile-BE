using MediatR;

namespace Application.Documents.Queries.GetAllDocumentsPaginated;

public record GetAllDocumentsPaginatedQuery : IRequest<IEnumerable<DocumentItemDto>>
{
    public Guid? RoomId { get; set; }
    public Guid? LockerId { get; set; }
    public Guid? FolderId { get; set; }
    public int? Page { get; set; }
    public int? Size { get; set; }
    public string? SortBy { get; set; }
}