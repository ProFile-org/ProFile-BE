using Application.Common.Models.Dtos.Physical;
using MediatR;

namespace Application.Staffs.Commands;

public class RemoveStaffFromRoom
{
    public record Command : IRequest<StaffDto>
    {
        public Guid StaffId { get; init; }
    }
}