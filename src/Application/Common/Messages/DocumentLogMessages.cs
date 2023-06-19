namespace Application.Common.Messages;

public static class DocumentLogMessages
{
    public static class Import
    {
        public const string NewImport = "Imported new document";
        public const string NewImportRequest = "Created new import request";
        public const string Checkin = "Checked in document";
        public const string Approve = "Document is approved to be imported";
        public const string Reject = "Rejected import request";
        public const string Assign = "Assigned to a folder";
    }
    public static class Borrow
    {
        public const string NewBorrowRequest = "Created new borrow request";
        public const string CanCel = "Cancelled borrow request";
        public const string Approve = "Approved borrow request";
        public const string Reject = "Rejected borrow request";
        public const string Checkout = "Checked out borrow request";
        public const string Return = "Returned borrow request";
        public const string Update = "Updated borrow request";
    }
    public const string Delete = "Delete document";
    public const string Update = "Updated document information";
    public static string GrantRead(string userName) => $"Share Read Permission to user {userName}";
    public static string GrantBorrow(string userName) => $"Share Borrow Permission to user {userName}";
    public static string RevokeRead(string userName) => $"Remove Read Permission to user {userName}";
    public static string RevokeBorrow(string userName) => $"Remove Borrow Permission to user {userName}";
}