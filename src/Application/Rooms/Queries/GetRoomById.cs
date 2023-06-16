using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Rooms.Queries;

public class GetRoomById
{
    public record Query : IRequest<RoomDto>
    {
        public string CurrentUserRole { get; init; } = null!;
        public Guid CurrentUserDepartmentId { get; init; }
        public Guid RoomId { get; init; }
    }
    
    public class QueryHandler : IRequestHandler<Query, RoomDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public QueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<RoomDto> Handle(Query request, CancellationToken cancellationToken)
        {
            var room = await _context.Rooms
                .Include(x => x.Department)
                .Include(x => x.Staff)
                .FirstOrDefaultAsync(x => x.Id == request.RoomId, cancellationToken: cancellationToken);
            
            if (room is null)
            {
                throw new KeyNotFoundException("Room does not exist.");
            }
            
            if (request.CurrentUserRole.IsStaff()
                && !IsSameDepartment(request.CurrentUserDepartmentId, room.DepartmentId))
            {
                throw new UnauthorizedAccessException("User cannot update this resource.");
            }
            
            return _mapper.Map<RoomDto>(room);
        }
        
        private static bool IsSameDepartment(Guid departmentId1, Guid departmentId2)
            => departmentId1 == departmentId2;
    }
}