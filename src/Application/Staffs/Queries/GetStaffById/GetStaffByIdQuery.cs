using Application.Users.Queries.Physical;
using MediatR;

namespace Application.Staffs.Queries.GetStaffById;

public record GetStaffByIdQuery : IRequest<StaffDto>
{
    public Guid StaffId { get; init; }
}