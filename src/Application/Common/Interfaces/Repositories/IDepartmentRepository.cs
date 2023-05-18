using Domain.Entities;

namespace Application.Common.Interfaces.Repositories;

public interface IDepartmentRepository
{
    Task<Department> GetByIdAsync(Guid id);
}