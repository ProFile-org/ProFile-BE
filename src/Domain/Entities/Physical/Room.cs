using Domain.Common;

namespace Domain.Entities.Physical;

public class Room : BaseEntity
{
    public string Name { get; set; }
    public string Description { get; set; }
    public Staff? Staff { get; set; }
    public int NumberOfLockers { get; set; }
}