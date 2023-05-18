using Domain.Entities;

namespace Application.Common.Interfaces.Repositories;

public interface IDepartmentRepository
{
    Task<Department> CreateDepartmentAsync(Department department);
    Task<Department> GetByIdAsync(Guid id);
}