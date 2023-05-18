using System.Data;
using Application.Common.Interfaces.Repositories;
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
    }

    public async Task<Department> CreateDepartmentAsync(Department department)
    {
        return new Department();
    }

    public async Task<Department?> GetByIdAsync(Guid id)
    {
       
        var sql = @"SELECT Id, Name FROM Departments WHERE Id = @id";
        return await _connection.QuerySingleOrDefaultAsync<Department>(sql, new { id });
    }
}