using Application.Common.Interfaces;
using Application.Common.Models.Dtos;
using Application.Common.Models.Dtos.Physical;
using Application.Identity;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Departments.Queries;

public class GetDepartmentById
{
    public record Query : IRequest<DepartmentDto>
    {
        public string UserRole { get; init; } = null!;
        public Guid UserDepartmentId { get; init; }
        public Guid DepartmentId { get; init; }
    }
    public class QueryHandler : IRequestHandler<Query, DepartmentDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public QueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<DepartmentDto> Handle(Query request, CancellationToken cancellationToken)
        {
            if ((request.UserRole.Equals(IdentityData.Roles.Staff) || request.UserRole.Equals(IdentityData.Roles.Employee)) 
                && request.UserDepartmentId != request.DepartmentId)
            {
                throw new UnauthorizedAccessException("User cannot access this department.");
            }
            
            var department = await _context.Departments.FirstOrDefaultAsync(x => x.Id.Equals(request.DepartmentId), cancellationToken);

            if (department is null)
            {
                throw new KeyNotFoundException("Department does not exist.");
            }

            return _mapper.Map<DepartmentDto>(department);
        }
    }
    
}