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
                  "User(username, email, password_hash, firstName, " +
                  "lastName, department_id, role, position, isActive) " +
                  "VALUES (@username, @email, @password_hash, @firstName," +
                  "@lastname, @department_id, @role, @position, @isActive)";
        var queryArguments = new
        {
            username = user.Username,
            email = user.Email,
            password_hash = user.PasswordHash,
            firstName = user.FirstName,
            lastName = user.LastName,
            department_id = user.Department.Id,
            role = user.Role,
            position = user.Position,
            isActive = user.IsActive
        };

        await _connection.ExecuteAsync(sql, queryArguments, transaction: _transaction);
        return user;
    }
}