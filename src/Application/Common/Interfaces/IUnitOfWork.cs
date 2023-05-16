namespace Application.Common.Interfaces;

public interface IUnitOfWork : IDisposable
{    
    IUserRepository UserRepository { get; }
    Task Commit();
}