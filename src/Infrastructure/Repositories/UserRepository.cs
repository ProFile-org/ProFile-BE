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
                  "Users(Id, Username, Email, PasswordHash, FirstName,LastName, DepartmentId, Role, Position, IsActive, IsActivated) " +
                  "VALUES(@id, @username, @email, @password_hash, @firstName,@lastName, @department_id, @role, @position, @isActive, @isActivated) " +
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
            isActivated = user.IsActivated
        };
        var insertedId = await _connection.ExecuteScalarAsync<Guid>(sql, queryArguments, transaction: _transaction);
        user.Id = insertedId;
        return user;
    }

    public async Task<User?> GetUserByIdAsync(Guid id)
    {
        var sql =
            @"SELECT Username, Email, FirstName, LastName, DepartmentId, Role, Position, IsActive, IsActivated " +
            "FROM Users " +
            "WHERE Id = @id";
        return await _connection.QueryFirstOrDefaultAsync<User?>(sql, new { id });
    }

    public async Task<IQueryable<User>> GetUsersByNameAsync(String firstName)
    {
        var sql = @"SELECT Username, Email, FirstName, LastName, DepartmentId, Role, Position, IsActive, IsActivated " +
                  "FROM Users " +
                  "WHERE FirstName = @firstName";
        var result = await _connection.QueryAsync<User>(sql, new { firstName });
        return result.AsQueryable();
    }

    public async Task<User> DisableUserById(Guid id)
    {
        var sql = @"UPDATE users " +
                  "SET IsActive = False " +
                  "WHERE id = @id";
        var insertedId = await _connection.ExecuteScalarAsync<Guid>(sql, new { id }, _transaction);
        var result = await GetUserByIdAsync(insertedId);
        return result;
    }
}