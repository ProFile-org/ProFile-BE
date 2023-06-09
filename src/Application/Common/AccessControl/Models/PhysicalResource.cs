namespace Application.Common.AccessControl.Models;

public struct PhysicalResource {
    public Guid Id { get; set;}

    public ResourceType Type { get; set;}
}

public enum ResourceType{
    Document,
    Folder,
    Locker,
}