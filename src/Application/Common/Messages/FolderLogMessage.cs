namespace Application.Common.Messages;

public static class FolderLogMessage
{
    public const string Add = "Add folder with Id {FolderId} to locker {LockerId} in room {RoomId} in department {DepartmentName}";
    public const string Update = "Updated folder with Id {FolderId} to locker {LockerId} in room {RoomId} in department {DepartmentName}";
    public const string Remove = "Removed folder with Id {FolderId} to locker {LockerId} in room {RoomId} in department {DepartmentName}";
    public const string AssignDocument = "Assigned document with Id {DocumentId} to folder with Id {FolderId}";
}