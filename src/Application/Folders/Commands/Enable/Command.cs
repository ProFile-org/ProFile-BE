using Application.Common.Models.Dtos.Physical;
using MediatR;

namespace Application.Folders.Commands.Enable;

public record Command : IRequest<FolderDto>
{
    public Guid FolderId { get; init; }
}
