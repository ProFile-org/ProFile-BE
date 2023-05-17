using Application.Common.Interfaces.Repositories;

namespace Application.Common.Interfaces;

public interface IUnitOfWork : IDisposable
{    
    IUserRepository UserRepository { get; }
    IDepartmentRepository DepartmentRepository { get; }
    IStaffRepository StaffRepository { get; }
    IRoomRepository RoomRepository { get; }
    Task Commit();
}