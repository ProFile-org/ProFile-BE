using Application.Users.Commands.CreateUserCommand;
using Application.Users.Queries;
using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IUserRepository
{
    Task<User> CreateUserAsync(User user);
    Task<IEnumerable<User>> GetUserByNameAsync(String firstName);
}