namespace Application.Common.Messages;

public class EntryLogMessages
{
    public const string DeleteBinEntry = "User with username {Username} delete entry with Id {EntryId}";
    public const string DownloadDigitalFile = "User with username {Username} download file with Id {FileId}";
    public const string MoveEntryToBin = "User with username {Username} move entry with Id {EntryId} to bin";
    public const string RestoreBinEntry = "User with username {Username} restore entry with Id {EntryId} from bin";
    public const string ShareEntry = "User with username {Username} change permission of entry with Id {EntryId} to User with id {SharedUsername}";
    public const string UpdateEntry = "User with username {Username} update entry with Id {EntryId}";
    public const string UploadDigitalFile = "User with username {Username} upload an entry with id {EntryId}";
    public const string UploadSharedEntry = "User with username {Username} upload an emtry with id {EntryId}";
}