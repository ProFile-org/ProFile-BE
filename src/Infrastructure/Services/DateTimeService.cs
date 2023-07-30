using Application.Common.Interfaces;

namespace Infrastructure.Services;

public class DateTimeService : IDateTimeProvider
{
    public DateTime DateTimeNow => DateTime.Now;
}