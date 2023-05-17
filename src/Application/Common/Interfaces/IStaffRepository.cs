using Domain.Entities.Physical;

namespace Application.Common.Interfaces;

public interface IStaffRepository
{
    Task<Staff?> GetByIdAsync(Guid id);
}