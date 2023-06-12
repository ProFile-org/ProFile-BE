using Domain.Entities;
using NodaTime;

namespace Domain.Common;

public class BaseLoggingEntity<T> : BaseEntity
    where T : BaseEntity
{
    public string Action { get; set; } = null!;
    public Guid UserId { get; set; }
    public T? Object { get; set; }
    public LocalDateTime Time { get; set; }

    public User User { get; set; } = null!;
}