using Domain.Entities;

namespace Application.Common.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User> CreateUserAsync(User user);
    Task<IEnumerable<User>> GetUserByNameAsync(String firstName);
}