using Domain.Common;
using Domain.Entities.Physical;

namespace Domain.Entities.Logging;

public class DocumentLog : BaseLoggingEntity
{
    public Folder? BaseFolder { get; set; }
}