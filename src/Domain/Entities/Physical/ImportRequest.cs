using Domain.Common;
using Domain.Statuses;
using NodaTime;

namespace Domain.Entities.Physical;

public class ImportRequest : BaseAuditableEntity
{
    public Room Room { get; set; } = null!;
    public Document Document { get; set; } = null!;
    public string Reason { get; set; } = null!;
    public ImportRequestStatus Status { get; set; }
}