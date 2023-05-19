using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Users.Queries;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Departments.Commands.DeleteDepartment;

public record DeleteDepartmentCommand : IRequest<DepartmentDto>
{
    public Guid DepartmentId { get; init; }
}

public class DeleteDepartmentCommandHandler : IRequestHandler<DeleteDepartmentCommand, DepartmentDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    public DeleteDepartmentCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    
    public async Task<DepartmentDto> Handle(DeleteDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = await _context.Departments.FirstOrDefaultAsync(x => x.Id == request.DepartmentId, cancellationToken);

        if (department is null)
        {
            throw new KeyNotFoundException("Department does not exist");
        }

        var result = _context.Departments.Remove(department);
        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<DepartmentDto>(result.Entity);
    }
} 