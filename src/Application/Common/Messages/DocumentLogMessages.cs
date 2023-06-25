namespace Application.Common.Messages;

public static class DocumentLogMessages
{
    public static class Import
    {
        public const string NewImport = "Imported new document with id {DocumentId}";
        public const string NewImportRequest = "Created new import request";
        public const string Checkin = "Checked in document with id {DocumentId}";
        public const string Approve = "Document with id {DocumentId} is approved to be imported";
        public const string Reject = "Document with id {DocumentId} is rejected to be imported";
        public const string Assign = "Assigned document with id {DocumentId} to folder {FolderId}";
    }
    public static class Borrow
    {
        public const string NewBorrowRequest = "Create new borrow request for document {DocumentId} with id {BorrowId}";
        public const string Cancel = "Cancel borrow request for document {DocumentId} with id {BorrowId}";
        public const string Approve = "Approve borrow request for document {DocumentId} with id {BorrowId}";
        public const string Reject = "Reject borrow request for document {DocumentId} with id {BorrowId}";
        public const string Checkout = "Check out borrow request for document {DocumentId} with id {BorrowId}";
        public const string Return = "Return borrow request for document {DocumentId} with id {BorrowId}";
        public const string UpdateBorrow = "Update borrow request for document {DocumentId} with id {BorrowId}";
    }
    public const string Delete = "Delete document with id {DocumentId}";
    public const string Update = "Update document with id {DocumentId} ";
    public const string Grant = "Share Permission {Permission} to user {Username}";
    public const string Revoke = "Remove Permission {Permission} from user {Username}";
}