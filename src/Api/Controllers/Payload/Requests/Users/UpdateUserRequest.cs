namespace Api.Controllers.Payload.Requests.Users;

/// <summary>
/// Request details to update a user
/// </summary>
public class UpdateUserRequest
{
    /// <summary>
    /// New first name of the user to be updated
    /// </summary>
    public string? FirstName { get; set; }
    /// <summary>
    /// New last name of the user to be updated
    /// </summary>
    public string? LastName { get; set; }
    /// <summary>
    /// New position of the user to be updated
    /// </summary>
    public string? Position { get; set; }
    public string Role { get; set; } = null!;
    public bool IsActive { get; set; }
}