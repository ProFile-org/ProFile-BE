using Domain.Common;
using Domain.Entities.Physical;

namespace Domain.Entities.Logging;

public class DocumentLog : BaseLoggingEntity<Document>
{
    public Folder? BaseFolder { get; set; }
}