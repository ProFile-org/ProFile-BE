using Domain.Common;

namespace Domain.Entities.Physical;

public class ImportRequest : BaseAuditableEntity
{
    public Room Room { get; set; }
    public Document Document { get; set; }
}