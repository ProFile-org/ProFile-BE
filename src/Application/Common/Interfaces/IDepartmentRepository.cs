using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IDepartmentRepository
{
    Task<Department> GetByIdAsync(Guid id);
}