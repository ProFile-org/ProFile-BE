using Domain.Common;
using Domain.Entities.Physical;
using Domain.Enums;

namespace Domain.Entities.Logging;

public class RequestLog : BaseLoggingEntity<Document>
{
    public RequestType Type { get; set; }
}