using System.Data;
using Application.Common.Interfaces;
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
                  "Users(Username, Email, PasswordHash, FirstName,LastName, DepartmentId, Role, Position, IsActive, IsActivated) " +
                  "VALUES(@username, @email, @passwordHash, @firstName,@lastname, @departmentId, @role, @position, @isActive, @isActivated)";

        var queryArguments = new
        {
            username = user.Username,
            email = user.Email,
            passwordHash = user.PasswordHash,
            firstName = user.FirstName,
            lastName = user.LastName,
            departmentId = user.Department.Id,
            role = user.Role,
            position = user.Position,
            isActive = user.IsActive,
            isActivated = user.IsActivated
        };
        var insertedId = await _connection.QueryFirstOrDefaultAsync<Guid>(sql, queryArguments, transaction: _transaction);
        var insertedUser = await GetUserByIdAsync(insertedId);
        return insertedUser;
    }

    public async Task<User?> GetUserByIdAsync(Guid id)
    {
        var sql =
            @"SELECT Username, Email, PasswordHash, FirstName, LastName, DepartmentId, Role, Position, IsActive, IsActivated " +
            "FROM Users " +
            "WHERE Id = @id";
        return await _connection.QueryFirstOrDefaultAsync<User?>(sql, new { id });
    }
}