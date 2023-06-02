using Domain.Common;
using NodaTime;

namespace Domain.Entities.Physical;

public class Borrow : BaseEntity
{
    public User Borrower { get; set; } = null!;
    public Document Document { get; set; } = null!;
    public LocalDateTime BorrowTime { get; set; }
    public LocalDateTime DueTime { get; set; }
    public string Reason { get; set; } = null!;
    public bool IsApproved { get; set; }
}