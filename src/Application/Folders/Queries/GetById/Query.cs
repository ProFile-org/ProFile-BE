using Application.Common.Models.Dtos.Physical;
using MediatR;

namespace Application.Folders.Queries.GetById;

public record Query : IRequest<FolderDto>
{
    public Guid FolderId { get; init; }
}