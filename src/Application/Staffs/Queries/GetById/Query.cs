using Application.Common.Models.Dtos.Physical;
using MediatR;

namespace Application.Staffs.Queries.GetById;

public record Query : IRequest<StaffDto>
{
    public Guid StaffId { get; init; }
}