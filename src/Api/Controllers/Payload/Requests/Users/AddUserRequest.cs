namespace Api.Controllers.Payload.Requests.Users;

/// <summary>
/// Request details to add a user
/// </summary>
public class AddUserRequest
{
    /// <summary>
    /// Username of the user to be added
    /// </summary>
    public string Username { get; init; } = null!;
    /// <summary>
    /// Email of the user to be added
    /// </summary>
    public string Email { get; init; } = null!;
    /// <summary>
    /// First name of the user to be added
    /// </summary>
    public string? FirstName { get; init; }
    /// <summary>
    /// Last name of the user to be added
    /// </summary>
    public string? LastName { get; init; }
    /// <summary>
    /// Department of the user to be added
    /// </summary>
    public Guid DepartmentId { get; init; }
    /// <summary>
    /// Role of the user to be added
    /// </summary>
    public string Role { get; init; } = null!;
    /// <summary>
    /// Position of the user to be added
    /// </summary>
    public string? Position { get; init; }
}