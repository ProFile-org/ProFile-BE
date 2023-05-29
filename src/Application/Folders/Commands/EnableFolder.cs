using Application.Common.Models.Dtos.Physical;
using MediatR;

namespace Application.Folders.Commands;

public class EnableFolder
{
    public record Command : IRequest<FolderDto>
    {
        public Guid FolderId { get; init; }
    }
}