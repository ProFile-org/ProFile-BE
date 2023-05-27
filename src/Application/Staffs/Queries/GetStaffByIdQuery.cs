using Application.Users.Queries.Physical;
using MediatR;

namespace Application.Staffs.Queries;

public record GetStaffByIdQuery : IRequest<StaffDto>
{
    public Guid StaffId { get; init; }
}