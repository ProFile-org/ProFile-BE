using Application.Common.Models.Dtos.Physical;
using MediatR;

namespace Application.Folders.Queries.GetFolderById;

public record GetFolderByIdQuery : IRequest<FolderDto>
{
    public Guid FolderId { get; init; }
}