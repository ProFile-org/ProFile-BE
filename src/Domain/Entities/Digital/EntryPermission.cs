using NodaTime;

namespace Domain.Entities.Digital;

public class EntryPermission
{
    public Guid EmployeeId { get; set; }
    public Guid EntryId { get; set; }
    public string AllowedOperations { get; set; } = null!;
    public LocalDateTime ExpiryDateTime { get; set; }
    public bool IsSharedRoot { get; set; }

    public User Employee { get; set; } = null!;
    public Entry Entry { get; set; } = null!;
}