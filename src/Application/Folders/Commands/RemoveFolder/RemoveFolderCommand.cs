using Application.Common.Models.Dtos.Physical;
using MediatR;

namespace Application.Folders.Commands.RemoveFolder;

public record RemoveFolderCommand : IRequest<FolderDto>
{
    public Guid FolderId { get; init; }
}