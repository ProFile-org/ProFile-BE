using MediatR;

namespace Application.Documents.Queries.GetDocumentTypes;

public record GetDocumentTypesQuery : IRequest<IEnumerable<string>>;