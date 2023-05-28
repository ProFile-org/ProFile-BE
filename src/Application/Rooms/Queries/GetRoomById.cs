using Application.Common.Models.Dtos.Physical;
using MediatR;

namespace Application.Rooms.Queries;

public class GetRoomById
{
    public record Query : IRequest<RoomDto>
    {
        public Guid RoomId { get; init; }
    }
}