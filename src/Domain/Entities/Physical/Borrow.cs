using Domain.Common;
using NodaTime;

namespace Domain.Entities.Physical;

public class Borrow : BaseEntity
{
    public User Borrower { get; set; }
    public Document BorrowDocument { get; set; }
    public LocalDateTime BorrowTime { get; set; }
    public LocalDateTime DueTime { get; set; }
    public string Reason { get; set; }
}