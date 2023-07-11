namespace Application.Common.Messages;

public class EntryLogMessages
{
    public const string DeleteBinEntry = "User with Id {UserId} delete entry with Id {EntryId}";
    public const string DownloadDigitalFile = "User with Id {UserId} download file with Id {FileId}";
    public const string MoveEntryToBin = "User with Id {UserId} move entry with Id {EntryId} to bin";
    public const string RestoreBinEntry = "User with Id {UserId} restore entry with Id {EntryId} from bin";
    public const string ShareEntry = "User with Id {UserId} change permission of entry with Id {EntryId} to User with id {SharedUserId}";
    public const string UpdateEntry = "User with Id {UserId} update entry with Id {EntryId}";
    public const string UploadDigitalFile = "User with Id {UserId} upload an entry with id {EntryId}";
    public const string UploadSharedEntry = "User with Id {UserId} upload an emtry with id {EntryId}";
}