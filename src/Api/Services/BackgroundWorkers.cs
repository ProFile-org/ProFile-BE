using Application.Common.Interfaces;
using Domain.Statuses;
using Infrastructure.Identity.Authorization;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Api.Services;

public class BackgroundWorkers : BackgroundService
{   
    private readonly IServiceProvider _serviceProvider;

    public BackgroundWorkers(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var workers = new List<Task>
            {
                DisposeExpiredEntries(TimeSpan.FromSeconds(10), stoppingToken),
                DisposeExpiredPermissions(TimeSpan.FromSeconds(10), stoppingToken),
                HandleOverdueRequest(TimeSpan.FromMinutes(2), stoppingToken),
                HandleOnlineUsers(TimeSpan.FromSeconds(10), stoppingToken),
            };

            await Task.WhenAll(workers.ToArray());
        }
    }

    private async Task DisposeExpiredEntries(TimeSpan delay, CancellationToken stoppingToken)
    {
        var expiryLocalDateTime = LocalDateTime.FromDateTime(DateTime.Now).PlusDays(-30);
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var entries = context.Entries.Where(x =>
            !EF.Functions.ILike(x.Path, "/%") && x.LastModified!.Value < expiryLocalDateTime);
        context.Entries.RemoveRange(entries);
        await context.SaveChangesAsync(stoppingToken);
        await Task.Delay(delay, stoppingToken);
    }
    
    private async Task DisposeExpiredPermissions(TimeSpan delay, CancellationToken stoppingToken)
    {
        var localDateTimeNow = LocalDateTime.FromDateTime(DateTime.Now);
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var expiredPermissions = context.Permissions.Where(x => x.ExpiryDateTime < localDateTimeNow);
        context.Permissions.RemoveRange(expiredPermissions);
        await context.SaveChangesAsync(stoppingToken);
        await Task.Delay(delay, stoppingToken);
    }

    private async Task HandleOverdueRequest(TimeSpan delay, CancellationToken stoppingToken)
    {
        var localDateTimeNow = LocalDateTime.FromDateTime(DateTime.Now);
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var overdueRequests = context.Borrows
            .Where(x => x.Status != BorrowRequestStatus.Overdue
                        && x.Status == BorrowRequestStatus.CheckedOut
                        && x.DueTime < localDateTimeNow)
            .ToList();

        foreach (var request in overdueRequests)
        {
            request.Status = BorrowRequestStatus.Overdue;
        }
        context.Borrows.UpdateRange(overdueRequests);

        await context.SaveChangesAsync(stoppingToken);
        await Task.Delay(delay, stoppingToken);
    }
    
    private async Task HandleOnlineUsers(TimeSpan delay, CancellationToken stoppingToken)
    {
        var onlineUsers = RequiresRoleAttribute.OnlineUsers;
        foreach (var id in onlineUsers.Keys
                                    .Where(id => DateTime.Now - onlineUsers[id] < delay))
        {
            RequiresRoleAttribute.OnlineUsers.Remove(id);
        }
        await Task.Delay(delay, stoppingToken);
    }
}