using Application.Common.Models.Dtos.Physical;
using MediatR;

namespace Application.Rooms.Commands;

public class EnableRoom
{
    public record Command : IRequest<RoomDto>
    {
        public Guid RoomId { get; init; }
    }
}