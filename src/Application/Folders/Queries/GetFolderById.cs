using Application.Common.Models.Dtos.Physical;
using MediatR;

namespace Application.Folders.Queries;

public class GetFolderById
{
    public record Query : IRequest<FolderDto>
    {
        public Guid FolderId { get; init; }
    }
}