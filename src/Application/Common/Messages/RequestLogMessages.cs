namespace Application.Common.Messages;

public static class RequestLogMessages
{
    public const string AddImportRequest = "Created new import request with id {RequestId}";
    public const string ApproveImport = "Approved import request with id {RequestId}";
    public const string RejectImport = "Rejected import request with id {RequestId}";
    public const string ApproveBorrow = "Rejected borrow request with id {RequestId}";
    public const string RejectBorrow = "Rejected borrow request with id {RequestId}";
    public const string CheckInImport = "Checked in import request with id {RequestId}";
}