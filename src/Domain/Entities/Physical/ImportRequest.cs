using Domain.Common;
using Domain.Statuses;

namespace Domain.Entities.Physical;

public class ImportRequest : BaseAuditableEntity
{
    public Guid RoomId { get; set; }
    public Guid DocumentId { get; set; }
    public string ImportReason { get; set; } = null!;
    public string StaffReason { get; set; } = null!;
    public ImportRequestStatus Status { get; set; }

    public Room Room { get; set; } = null!;
    public Document Document { get; set; } = null!;
}