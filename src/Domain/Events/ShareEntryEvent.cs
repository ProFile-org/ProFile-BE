using Domain.Common;

namespace Domain.Events;

public class ShareEntryEvent : BaseEvent
{
    public ShareEntryEvent(string entryName, string sharerName, string ownerName, string sharedUserEmail, bool isDirectory, string operation, string path)
    {
        EntryName = entryName;
        SharerName = sharerName;
        OwnerName = ownerName;
        SharedUserEmail = sharedUserEmail;
        IsDirectory = isDirectory;
        Operation = operation;
        Path = path;
    }
    
    public string EntryName { get; }
    public string SharerName { get; }
    public string OwnerName { get; }
    public string SharedUserEmail { get; }
    public bool IsDirectory { get; }
    public string Operation { get; }
    public string Path { get; }
}