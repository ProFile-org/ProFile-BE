using NodaTime;

namespace Domain.Entities.Physical;

public class Permission
{
    public Guid EmployeeId { get; set; }
    public Guid DocumentId { get; set; }
    public string AllowedOperations { get; set; } = null!;
    public LocalDateTime ExpiryDateTime { get; set; }

    public User Employee { get; set; } = null!;
    public Document Document { get; set; } = null!;
}