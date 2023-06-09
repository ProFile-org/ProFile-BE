﻿namespace Api.Controllers.Payload.Requests;

/// <summary>
/// Get all logs paginated
/// </summary>
public class GetAllLogsPaginatedQueryParameters
{
    /// <summary>
    /// Search term
    /// </summary>
    public string? SearchTerm { get; set; }
    /// <summary>
    /// Page number
    /// </summary>
    public int? Page { get; set; }
    /// <summary>
    /// Size number
    /// </summary>
    public int? Size { get; set; }
    /// <summary>
    /// Object Id
    /// </summary>
    public Guid? ObjectId { get; set; }
    /// <summary>
    /// Object type
    /// </summary>
    public string ObjectType { get; set; } = null!;
}