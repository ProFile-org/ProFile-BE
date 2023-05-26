using Application.Common.Models.Dtos.Physical;
using MediatR;

namespace Application.Rooms.Queries.GetRoomById;

public record GetRoomByIdQuery : IRequest<RoomDto>
{
    public Guid RoomId { get; init; }
}