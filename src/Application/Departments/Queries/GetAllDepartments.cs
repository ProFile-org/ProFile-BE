using System.Collections.ObjectModel;
using Application.Common.Interfaces;
using Application.Common.Models.Dtos;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Departments.Queries;

public class GetAllDepartments
{
    public record Query : IRequest<IEnumerable<DepartmentDto>>;

    public class QueryHandler : IRequestHandler<Query, IEnumerable<DepartmentDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public QueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<DepartmentDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var departments = await _context.Departments
                // .Where(x => !x.Name.Equals("Admin"))
                .ToListAsync(cancellationToken);
            var result = new ReadOnlyCollection<DepartmentDto>(_mapper.Map<List<DepartmentDto>>(departments));
            return result;
        }
    } 
}
