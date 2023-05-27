using Application.Common.Models.Dtos.Physical;
using MediatR;

namespace Application.Folders.Commands.UpdateFolder;

public record UpdateFolderCommand : IRequest<FolderDto>
{
    public Guid FolderId { get; init; }
    public string Name { get; init; } = null!;
    public string Description { get; init; } = null!;
    public int Capacity { get; init; }
}