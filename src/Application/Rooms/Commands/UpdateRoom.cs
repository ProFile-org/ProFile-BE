using Application.Common.Models.Dtos.Physical;
using MediatR;

namespace Application.Rooms.Commands;

public class UpdateRoom
{
    public record Command : IRequest<RoomDto>
    {
        public Guid RoomId { get; init; }
        public string Name { get; init; } = null!;
        public string? Description { get; init; }
        public int Capacity { get; init; }
    }
}