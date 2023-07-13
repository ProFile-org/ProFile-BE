namespace Domain.Statuses;

public enum BorrowRequestStatus
{
    Pending,
    Approved,
    Rejected,
    CheckedOut,
    Returned,
    Overdue,
    Cancelled,
    Lost,
    NotProcessable,
}