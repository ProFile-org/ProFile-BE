using Domain.Entities;
using Domain.Entities.Physical;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Interfaces;

public interface IApplicationDbContext
{
    public DbSet<Department> Departments { get;  }
    public DbSet<User> Users { get; }
    public DbSet<Staff> Staffs { get; }
    public DbSet<Room> Rooms { get; }
    public DbSet<Locker> Lockers { get; }
    public DbSet<Folder> Folders { get; }
    public DbSet<Document> Documents { get; }
    public DbSet<Borrow> Borrows { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}