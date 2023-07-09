using Serilog.Context;

namespace Application.Common.Logging;

public class Logging : IDisposable
{
    private IDisposable ObjectType { get; }
    private IDisposable ObjectId { get; }
    private IDisposable UserId { get; }

    private Logging(string objectType, Guid objectId, Guid userId)
    {
        ObjectType = LogContext.PushProperty("ObjectType", objectType);
        ObjectId = LogContext.PushProperty("ObjectId", objectId);
        UserId = LogContext.PushProperty("UserId", userId);
    }

    public static Logging PushProperties(string objectType, Guid objectId, Guid userId)
        => new(objectType, objectId, userId);
    public void Dispose()
    {
        ObjectType.Dispose();
        ObjectId.Dispose();
        UserId.Dispose();
        GC.SuppressFinalize(this);
    }
}