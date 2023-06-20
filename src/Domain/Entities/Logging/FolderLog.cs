using Domain.Common;
using Domain.Entities.Physical;

namespace Domain.Entities.Logging;

public class FolderLog : BaseLoggingEntity
{
    public Locker? BaseLocker { get; set; }
}