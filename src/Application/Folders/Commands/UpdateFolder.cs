using Application.Common.Models.Dtos.Physical;
using MediatR;

namespace Application.Folders.Commands;

public class UpdateFolder
{
    public record Command : IRequest<FolderDto>
    {
        public Guid FolderId { get; init; }
        public string Name { get; init; } = null!;
        public string? Description { get; init; }
        public int Capacity { get; init; }
    }
}