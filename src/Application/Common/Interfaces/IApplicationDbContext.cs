using Application.Common.Models;
using Domain.Entities;
using Domain.Entities.Digital;
using Domain.Entities.Logging;
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
    public DbSet<ImportRequest> ImportRequests { get; }
    public DbSet<Borrow> Borrows { get; }
    public DbSet<Permission> Permissions { get; }
    
    public DbSet<FileEntity> Files { get; }
    public DbSet<Entry> Entries { get; }
    
    public DbSet<Log> Logs { get; }

    public DbSet<RoomLog> RoomLogs { get; }
    public DbSet<LockerLog> LockerLogs { get; }
    public DbSet<FolderLog> FolderLogs { get; }
    public DbSet<DocumentLog> DocumentLogs { get; }
    public DbSet<RequestLog> RequestLogs { get; }
    public DbSet<UserLog> UserLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}