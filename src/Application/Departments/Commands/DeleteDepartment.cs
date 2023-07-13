using Application.Common.Interfaces;
using Application.Common.Models.Dtos;
using Application.Users.Queries;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Departments.Commands;

public class DeleteDepartment
{
    public record Command : IRequest<DepartmentDto>
    {
        public Guid DepartmentId { get; init; }
    }

    public class CommandHandler : IRequestHandler<Command, DepartmentDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        public CommandHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
    
        public async Task<DepartmentDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var department = await _context.Departments.FirstOrDefaultAsync(x => x.Id == request.DepartmentId, cancellationToken);

            if (department is null)
            {
                throw new KeyNotFoundException("Department does not exist.");
            }

            var result = _context.Departments.Remove(department);
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<DepartmentDto>(result.Entity);
        }
    } 
}