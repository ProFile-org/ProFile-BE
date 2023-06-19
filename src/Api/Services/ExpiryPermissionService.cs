using Application.Common.Interfaces;
using Domain.Entities.Physical;
using NodaTime;

namespace Api.Services;

public class ExpiryPermissionService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public ExpiryPermissionService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var localDateTimeNow = LocalDateTime.FromDateTime(DateTime.Now);
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

            var expiredPermissions = context.Permissions.Where(x => x.ExpiryDateTime < localDateTimeNow);
            context.Permissions.RemoveRange(expiredPermissions);
            await context.SaveChangesAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}