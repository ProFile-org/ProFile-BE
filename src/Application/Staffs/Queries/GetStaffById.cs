using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Staffs.Queries;

public class GetStaffById
{
    public record Query : IRequest<StaffDto>
    {
        public Guid StaffId { get; init; }
    }
    
    public class QueryHandler : IRequestHandler<Query, StaffDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public QueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<StaffDto> Handle(Query request, CancellationToken cancellationToken)
        {
            var staff = await _context.Staffs
                .Include(x => x.User)
                .Include(y => y.Room)
                .FirstOrDefaultAsync(x => x.Id.Equals(request.StaffId), cancellationToken: cancellationToken);

            if (staff is null)
            {
                throw new KeyNotFoundException("Staff does not exist.");
            }

            return _mapper.Map<StaffDto>(staff);
        }
    }
}