using Application.Common.Models.Dtos.Physical;
using MediatR;

namespace Application.Rooms.Commands.UpdateRoom;

public record UpdateRoomCommand : IRequest<RoomDto>
{
    public Guid RoomId { get; init; }
    public string? Description { get; init; }
    public Guid? StaffId { get; init; }
    public int? Capacity { get; init; }
}