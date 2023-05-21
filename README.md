![](https://github.com/ProFile-org/ProFile-BE/actions/workflows/dev-ci.yml/badge.svg)
![](https://github.com/ProFile-org/ProFile-BE/actions/workflows/shiping.yml/badge.svg)
# ProFile - Backend

## To create new repository
### Steps
- Add new repository interface to `src/Application/Common/Interfaces/Repositories` including necessary operations
```c#
public interface ISampleRepository
{
    Task<int> GetTests();
}
```
- Add a property of that interface type to `src/Application/Common/Interfaces/IUnitOfWork.cs` like this:

```c#
public interface IUnitOfWork : IDisposable
{
    ...
    ISampleRepository SampleRepository { get; }
    ...
}
```
- Implement that repo interface in `src/Infrastructure/Repositories/YourRepository.cs`
```c#
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
```
- THIS IS A BIG NOTE: Always inject IDbConnection into the repository, if your repo include command operations (Add, Update, Delete) then inject IDbTransaction also, and do as sample
- With query operations: Use QueryAsync<>()
- With command operations: Use ExecuteAsync()
- Register that repo in unit of work:
```c#
public class UnitOfWork : IUnitOfWork
{
    ...
    public UnitOfWork(... , ISampleRepository sampleRepository)
    {
        // Baseline
        ...

        // Inject repositories here
        SampleRepository = sampleRepository;
    }
    
    // Add repositories here
    
    public ISampleRepository SampleRepository { get; }
    
    // End of adding repositories
```
 - Register that repo again in `src/Infrastructure/ConfigureServices.cs`:
```c#
public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        ...
        services.RegisterRepositories();
        
        return services;
    }

    private static IServiceCollection RegisterRepositories(this IServiceCollection services)
    {
        // Always register with scoped lifetime here
        services.AddScoped<ISampleRepository, SampleRepository>();
        return services;
    }
}
```

## To use repository with MediatR and CQRS
### Steps
- To create a command/query related to an entity: create folders in structure of `/src/Application/Entities/Commands/Function/FunctionCommand`. <br/>
For example: `src/Application/Tests/Commands/GetTests/GetTestsCommand`, the same with queries
- Add a `GetTestsCommand.cs` to the folder created:
```c#
public record GetTestsCommand : IRequest<int>
{
    
}
```
- A command/query implements an IRequest< TResult> with TResult being the result of the operation you want
- You can use either record or class, but record is more preferred
- Add any properties needed for the operation
- Add a request handler to the same file:
```c#
public record GetTestsCommand : IRequest<int>
{
    
}

public class GetTestsCommandHandler : IRequestHandler<GetTestsCommand, int>
{
    private readonly IUnitOfWork _uow;

    public GetTestsCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<int> Handle(GetTestsCommand request, CancellationToken cancellationToken)
    {
        // Handle
    }
} 
```
- This handler must implement IRequestHandler with type parameters being 
  - The command/query you created
  - The TResult you used in the IRequest implementation
- Remember to inject the IUnitOfWork interface, as this interface is your single point of entry to operate on your databases
- Use this `_uow` in your `Handle` method:
```c#
public async Task<int> Handle(GetTestsCommand request, CancellationToken cancellationToken)
{
    return await _uow.SampleRepository.GetTests();
}
```
- As your repository has been registered, and all services needed for your repositories and unit of work have also been registered, you can safely call the abstract methods without worrying about DI not working
