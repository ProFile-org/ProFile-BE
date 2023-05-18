using Application.Common.Interfaces;
using Application.Users.Queries;
using AutoMapper;
using Domain.Entities;
using MediatR;

namespace Application.Departments.Commands.CreateDepartment;

public record CreateDepartmentCommand : IRequest<DepartmentDto>
{
    public string Name { get; init; }
}

public class CreateDepartmentCommandHandler : IRequestHandler<CreateDepartmentCommand, DepartmentDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    public CreateDepartmentCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<DepartmentDto> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var entity = new Department
        {
            Name = request.Name
        };

        var result = await _context.Departments.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<DepartmentDto>(result);
    }
}   