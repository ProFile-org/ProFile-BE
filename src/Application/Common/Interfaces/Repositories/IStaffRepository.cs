using Domain.Entities.Physical;

namespace Application.Common.Interfaces.Repositories;

public interface IStaffRepository
{
    Task<Staff?> GetByIdAsync(Guid id);
}