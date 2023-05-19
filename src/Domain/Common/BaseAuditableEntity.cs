using NodaTime;

namespace Domain.Common;

public abstract class BaseAuditableEntity : BaseEntity
{
    public LocalDateTime Created { get; set; }

    public Guid? CreatedBy { get; set; }

    public LocalDateTime? LastModified { get; set; }

    public Guid? LastModifiedBy { get; set; }
}
