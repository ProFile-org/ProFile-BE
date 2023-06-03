namespace Domain.Statuses;

public enum BorrowRequestStatus
{
    Approved,
    Pending,
    Rejected,
    Overdue,
    Cancelled,
    CheckedOut,
    Returned,
    Lost,
    NotProcessable,
}