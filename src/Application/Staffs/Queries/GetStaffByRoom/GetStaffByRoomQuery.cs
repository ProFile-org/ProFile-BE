using Application.Users.Queries.Physical;
using MediatR;

namespace Application.Staffs.Queries.GetStaffByRoom;

public record GetStaffByRoomQuery : IRequest<StaffDto>
{
    public Guid RoomId { get; init; }
}