using System.Collections.ObjectModel;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Dtos.Physical;
using Application.Identity;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Rooms.Queries;

public class GetRoomByDepartmentId
{
    public record Query : IRequest<ItemsResult<RoomDto>>
    {
        public string CurrentUserRole { get; init; } = null!;
        public Guid CurrentUserDepartmentId { get; init; }
        public Guid DepartmentId { get; init; }
    }
    
    public class QueryHandler : IRequestHandler<Query, ItemsResult<RoomDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public QueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ItemsResult<RoomDto>> Handle(Query request, CancellationToken cancellationToken)
        {

            if ((request.CurrentUserRole.IsEmployee() 
                || request.CurrentUserRole.IsStaff())
                && request.CurrentUserDepartmentId != request.DepartmentId)
            {
                throw new UnauthorizedAccessException("User cannot access this resource.");
            }

            var department = await _context.Departments
                .FirstOrDefaultAsync(x => x.Id == request.DepartmentId, cancellationToken);

            if (department is null)
            {
                throw new KeyNotFoundException("Department does not exist.");
            }
            
            var rooms = _context.Rooms
                .Include(x => x.Department)
                .Include(x => x.Staff)
                .Where(x => x.DepartmentId == request.DepartmentId);
            
            var result = new ItemsResult<RoomDto>
            (new ReadOnlyCollection<RoomDto>(_mapper.Map<List<RoomDto>>(rooms)));
            return result;
        }
    }
}