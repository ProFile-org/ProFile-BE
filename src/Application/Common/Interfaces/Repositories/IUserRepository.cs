using Domain.Entities;

namespace Application.Common.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User> CreateUserAsync(User user);
    Task<IQueryable<User>> GetUsersByNameAsync(String firstName);
    Task<User?> GetUserByIdAsync(Guid id);
}