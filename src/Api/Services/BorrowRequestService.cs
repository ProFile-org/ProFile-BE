using Application.Common.Interfaces;
using Domain.Statuses;
using Infrastructure.Persistence;
using NodaTime;

namespace Api.Services;

public class BorrowRequestService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public BorrowRequestService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var localDateTimeNow = LocalDateTime.FromDateTime(DateTime.Now);
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var overdueRequests = context.Borrows
                    .Where(x => x.Status != BorrowRequestStatus.Overdue
                                && x.Status == BorrowRequestStatus.CheckedOut
                                && x.DueTime < localDateTimeNow)
                    .ToList();

                foreach (var request in overdueRequests)
                {
                    request.Status = BorrowRequestStatus.Overdue;
                    context.Borrows.Update(request);
                }

                await context.SaveChangesAsync(stoppingToken);
                Console.WriteLine("fuck you chien ngu");
                await Task.Delay(TimeSpan.FromMinutes(20), stoppingToken);
            }
        }
    }
}