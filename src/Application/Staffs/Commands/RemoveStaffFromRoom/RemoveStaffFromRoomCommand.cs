using Application.Users.Queries.Physical;
using MediatR;

namespace Application.Staffs.Commands.RemoveStaffFromRoom;

public record RemoveStaffFromRoomCommand : IRequest<StaffDto>
{
    public Guid StaffId { get; init; }
    public Guid RoomId { get; init; }
}