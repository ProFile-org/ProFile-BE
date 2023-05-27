using Application.Common.Models.Dtos.Physical;
using MediatR;

namespace Application.Rooms.Commands.Enable;

public record Command : IRequest<RoomDto>
{
    public Guid RoomId { get; init; }
}