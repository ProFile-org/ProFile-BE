using Domain.Common;

namespace Domain.Entities.Physical;

public class Folder : BaseEntity
{
    public string Name { get; set; }
    public string Description { get; set; }
    public Locker Locker { get; set; }
    public int Capacity { get; set; }
}