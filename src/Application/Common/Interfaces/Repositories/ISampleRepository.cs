namespace Application.Common.Interfaces.Repositories;

public interface ISampleRepository
{
    Task<int> GetTests();
}