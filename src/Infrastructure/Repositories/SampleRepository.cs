using System.Data;
using Application.Common.Interfaces.Repositories;
using Dapper;

namespace Infrastructure.Repositories;

public class SampleRepository : ISampleRepository
{
    private readonly IDbConnection _connection;
    private readonly IDbTransaction _transaction;

    public SampleRepository(IDbConnection connection, IDbTransaction transaction)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public async Task<int> GetTests()
    {
        var sql = "CREATE TABLE Documents(id INTEGER)";
        return await _connection.ExecuteAsync(sql, transaction: _transaction);
    }
}