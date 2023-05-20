using Domain.Common;

namespace Domain.Entities;

public class Department : BaseEntity
{
    public string Name { get; set; } = null!;
}