using System.Collections.ObjectModel;
using Application.Common.Interfaces;
using Application.Users.Queries;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Departments.Queries.GetAllDepartments;

public record GetAllDepartmentsQuery : IRequest<IEnumerable<DepartmentDto>>;

public class GetAllDepartmentsQueryHandler : IRequestHandler<GetAllDepartmentsQuery, IEnumerable<DepartmentDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAllDepartmentsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<DepartmentDto>> Handle(GetAllDepartmentsQuery request, CancellationToken cancellationToken)
    {
        var departments = await _context.Departments.ToListAsync(cancellationToken);
        var result = new ReadOnlyCollection<DepartmentDto>(_mapper.Map<List<DepartmentDto>>(departments));
        return result;
    }
} 