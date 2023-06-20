using Domain.Entities;
using NodaTime;

namespace Domain.Common;

public class BaseLoggingEntity : BaseEntity
{
    public string Action { get; set; } = null!;
    public Guid UserId { get; set; }
    public Guid? ObjectId { get; set; }
    public LocalDateTime Time { get; set; }

    public User User { get; set; } = null!;
}