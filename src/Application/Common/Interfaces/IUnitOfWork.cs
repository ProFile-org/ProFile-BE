using Application.Common.Interfaces.Repositories;

namespace Application.Common.Interfaces;

public interface IUnitOfWork : IDisposable
{    
    ISampleRepository SampleRepository { get;  }
    Task Commit();
}