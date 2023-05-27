using Application.Common.Models.Dtos.Physical;
using MediatR;

namespace Application.Rooms.Queries.GetById;

public record Query : IRequest<RoomDto>
{
    public Guid RoomId { get; init; }
}