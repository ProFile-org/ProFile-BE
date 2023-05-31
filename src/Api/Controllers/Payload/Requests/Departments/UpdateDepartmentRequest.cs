namespace Api.Controllers.Payload.Requests.Departments;

/// <summary>
/// Request details to update a department
/// </summary>
public class UpdateDepartmentRequest
{
    /// <summary>
    /// New name of the department to be updated
    /// </summary>
    public string Name { get; set; } = null!;
}