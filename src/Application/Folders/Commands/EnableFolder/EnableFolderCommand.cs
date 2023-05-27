using Application.Common.Models.Dtos.Physical;
using MediatR;

namespace Application.Folders.Commands.EnableFolder;

public record EnableFolderCommand : IRequest<FolderDto>
{
    public Guid FolderId { get; init; }
}
