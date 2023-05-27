using Application.Common.Models.Dtos.Physical;
using MediatR;

namespace Application.Rooms.Commands.EnableRoom;

public record EnableRoomCommand : IRequest<RoomDto>
{
    public Guid RoomId { get; init; }
}