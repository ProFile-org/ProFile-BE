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
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public CreateDepartmentCommandHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<DepartmentDto> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();

        while (await _uow.DepartmentRepository.GetByIdAsync(id) != null)
        {
            id = Guid.NewGuid();
        }
        
        var entity = new Department
        {
            Id = id,
            Name = request.Name
        };

        var result = await _uow.DepartmentRepository.CreateDepartmentAsync(entity);
        await _uow.Commit();
        return _mapper.Map<DepartmentDto>(result);
    }
}   