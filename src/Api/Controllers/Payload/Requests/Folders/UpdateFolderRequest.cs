namespace Api.Controllers.Payload.Requests.Folders;

/// <summary>
/// Request details to update a folder
/// </summary>
public class UpdateFolderRequest
{
    /// <summary>
    /// New name of the folder to be updated
    /// </summary>
    public string Name { get; set; } = null!;
    /// <summary>
    /// New description of the folder to be updated
    /// </summary>
    public string? Description { get; set; }
    /// <summary>
    /// New capacity of the folder to be updated
    /// </summary>
    public int Capacity { get; set; }
    /// <summary>
    /// Status of folder to be updated
    /// </summary>
    public bool IsAvailable { get; set; }
}