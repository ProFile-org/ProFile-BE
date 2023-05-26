using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Users.Queries;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Departments.Commands.AddDepartment;

public record AddDepartmentCommand : IRequest<DepartmentDto>
{
    public string Name { get; init; } = null!;
}

public class CreateDepartmentCommandHandler : IRequestHandler<AddDepartmentCommand, DepartmentDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    public CreateDepartmentCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<DepartmentDto> Handle(AddDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = await _context.Departments.FirstOrDefaultAsync(x => x.Name.Trim().ToLower().Equals(request.Name.Trim().ToLower()), cancellationToken);

        if (department is not null)
        {
            throw new ConflictException("Department name already exists.");
        }
        var entity = new Department
        {
            Name = request.Name
        };

        var result = await _context.Departments.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<DepartmentDto>(result.Entity);
    }
}   