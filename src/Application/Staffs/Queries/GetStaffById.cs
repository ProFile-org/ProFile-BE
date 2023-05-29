using Application.Common.Models.Dtos.Physical;
using MediatR;

namespace Application.Staffs.Queries;

public class GetStaffById
{
    public record Query : IRequest<StaffDto>
    {
        public Guid StaffId { get; init; }
    }
}