using Domain.Common;
using Domain.Entities.Physical;

namespace Domain.Entities.Logging;

public class LockerLog : BaseLoggingEntity<Locker>
{
    public Room? BaseRoom { get; set; }
}