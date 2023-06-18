using System.Reflection;
using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Entities.Digital;
using Domain.Entities.Logging;
using Domain.Entities.Physical;
using Infrastructure.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext, IAuthDbContext
{
    private readonly IMediator _mediator;
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options, 
        IMediator mediator) : base(options)
    {
        _mediator = mediator;
    }

    public DbSet<Department> Departments => Set<Department>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Staff> Staffs => Set<Staff>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Locker> Lockers => Set<Locker>();
    public DbSet<Folder> Folders => Set<Folder>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<ImportRequest> ImportRequests => Set<ImportRequest>();
    public DbSet<Borrow> Borrows => Set<Borrow>();
    public DbSet<Permission> Permissions => Set<Permission>();
    
    public DbSet<UserGroup> UserGroups => Set<UserGroup>();
    public DbSet<FileEntity> Files => Set<FileEntity>();
    public DbSet<Entry> Entries => Set<Entry>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<ResetPasswordToken> ResetPasswordTokens => Set<ResetPasswordToken>();
    
    public DbSet<RoomLog> RoomLogs => Set<RoomLog>();
    public DbSet<LockerLog> LockerLogs => Set<LockerLog>();
    public DbSet<FolderLog> FolderLogs => Set<FolderLog>();
    public DbSet<DocumentLog> DocumentLogs => Set<DocumentLog>();
    public DbSet<RequestLog> RequestLogs => Set<RequestLog>();
    public DbSet<UserLog> UserLogs => Set<UserLog>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Scan for entity configurations using FluentAPI
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _mediator.DispatchDomainEvents(this);

        return await base.SaveChangesAsync(cancellationToken);
    }
}