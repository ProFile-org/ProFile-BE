using System.Data;
using Application.Common.Interfaces.Repositories;
using Dapper;
using Domain.Entities;

namespace Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IDbConnection _connection;
    private readonly IDbTransaction _transaction;

    public UserRepository(IDbConnection connection, IDbTransaction transaction)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public async Task<User> CreateUserAsync(User user)
    {
        var sql = @"INSERT INTO " +
                  "Users(Id, Username, Email, PasswordHash, FirstName,LastName, DepartmentId, Role, Position, IsActive, IsActivated, Created) " +
                  "VALUES(@id, @username, @email, @passwordHash, @firstName,@lastname, @departmentId, @role, @position, @isActive, @isActivated, @created) " +
                  "RETURNING Id";
        var queryArguments = new
        {
            id = user.Id,
            username = user.Username,
            email = user.Email,

            lastName = user.LastName,
            password_hash = user.PasswordHash,
            firstName = user.FirstName,
            department_id = user.Department.Id,

            role = user.Role,
            position = user.Position,
            isActive = user.IsActive,
            isActivated = user.IsActivated,
            created = user.Created
        };
        var insertedId = await _connection.ExecuteScalarAsync<Guid>(sql, queryArguments, _transaction);
        var insertedUser = await GetUserByIdAsync(insertedId);
        return insertedUser;
    }

    public async Task<User?> GetUserByIdAsync(Guid id)
    {
        var sql =
            @"SELECT Id, Username, Email, FirstName, LastName, DepartmentId, Role, Position, IsActive, IsActivated, Created, CreatedBy, Modified, ModifiedBy " +
            "FROM Users " +
            "WHERE Id = @id";
        return await _connection.QueryFirstOrDefaultAsync<User?>(sql, new { id });
    }

    public Task<User> DisableUserById(Guid id)
    {
        // throw new NotImplementedException();
    }

    public async Task<IQueryable<User>> GetUsersByNameAsync(String firstName)
    {
        var sql = @"SELECT username, email, password_hash, first_name,last_name, department_id, role, position, is_active, is_activated " +
                  "FROM users " +
                  "WHERE first_name = @firstName";
        var result = await _connection.QueryAsync<User>(sql, new { firstName });
        return result.AsQueryable();
    }
}