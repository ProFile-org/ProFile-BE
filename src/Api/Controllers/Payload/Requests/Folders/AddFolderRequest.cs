namespace Api.Controllers.Payload.Requests.Folders;
/// <summary>
/// Request details to add a folder
/// </summary>
public class AddFolderRequest
{
    /// <summary>
    /// Name of the folder to be added
    /// </summary>
    public string Name { get; init; } = null!;
    /// <summary>
    /// Description of the folder to be added
    /// </summary>
    public string? Description { get; init; }
    /// <summary>
    /// Number of documents this folder can hold
    /// </summary>
    public int Capacity { get; init; }
    /// <summary>
    /// Id of the locker that this folder will be in
    /// </summary>
    public Guid LockerId { get; init; }
}