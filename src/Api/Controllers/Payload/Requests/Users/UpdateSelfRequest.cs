namespace Api.Controllers.Payload.Requests.Users;

/// <summary>
/// Request details to update that user
/// </summary>
public class UpdateSelfRequest
{
    /// <summary>
    /// New first name of the user to be updated
    /// </summary>
    public string? FirstName { get; set; }
    /// <summary>
    /// New last name of the user to be updated
    /// </summary>
    public string? LastName { get; set; }
}