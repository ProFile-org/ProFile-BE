namespace Api.Controllers.Payload.Requests.Departments;

/// <summary>
/// Request details to add a department
/// </summary>
public class AddDepartmentRequest
{
    /// <summary>
    /// Name of the department to be added
    /// </summary>
    public string Name { get; init; } = null!;
}