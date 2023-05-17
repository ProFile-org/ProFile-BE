using System.Data;
using Application.Common.Interfaces;
using Dapper;
using Domain.Entities;

namespace Infrastructure.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly IDbConnection _connection;
    private readonly IDbTransaction _transaction;

    public DepartmentRepository(IDbConnection connection, IDbTransaction transaction)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public async Task<Department> GetByIdAsync(Guid id)
    {
        var sql = @"SELECT id, name FROM Department WHERE id = @id";
        return await _connection.QuerySingleOrDefault<Task<Department>>(sql, new { id });
    }
}