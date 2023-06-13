namespace Application.Common.Messages;

public static class DocumentLogMessages
{
    public static class Import
    {
        public const string NewImport = "Imported new document";
        public const string NewImportRequest = "Created new import request";
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
}