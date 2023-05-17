using System.Data;
using Application.Common.Interfaces;
using Infrastructure.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly IDbTransaction _dbTransaction;
    private readonly IMediator _mediator;
    private readonly DbContext _context;

    public UnitOfWork(IDbTransaction dbTransaction, IMediator mediator, DbContext context, 
        IUserRepository userRepository,
        IDepartmentRepository departmentRepository)
    {
        // Baseline
        _dbTransaction = dbTransaction;
        _mediator = mediator;
        _context = context;

        // Inject repositories
        UserRepository = userRepository;
        DepartmentRepository = departmentRepository;
    }
    
    // Add Repositories
    
    // End of adding repositories

    public IUserRepository UserRepository { get; }
    public IDepartmentRepository DepartmentRepository { get; }

    public async Task Commit()
    {
        try
        {
            _dbTransaction.Commit();
            await _mediator.DispatchDomainEvents(_context);
        }
        catch (Exception ex)
        {
            _dbTransaction.Rollback();
        }
    }

    public void Dispose()
    {
        //Close the SQL Connection and dispose the objects
        _dbTransaction.Connection?.Close();
        _dbTransaction.Connection?.Dispose();
        _dbTransaction.Dispose();
    }
}