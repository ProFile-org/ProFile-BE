using Application.Common.Models.Dtos.Physical;
using MediatR;

namespace Application.Staffs.Commands.RemoveFromRoom;

public record Command : IRequest<StaffDto>
{
    public Guid StaffId { get; init; }
    public Guid RoomId { get; init; }
}