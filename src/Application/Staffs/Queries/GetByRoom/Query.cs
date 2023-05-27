using Application.Common.Models.Dtos.Physical;
using MediatR;

namespace Application.Staffs.Queries.GetByRoom;

public record Query : IRequest<StaffDto>
{
    public Guid RoomId { get; init; }
}