using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NodaTime;

namespace Application.Common.Models;

public class Log {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string? Template { get; set; }
    public string? Message { get; set; }
    public string? Level { get; set; }
    public Instant? Time { get; set; }
    public string? Event { get; set; }
    public Guid? UserId { get; set; }
    public Guid? ObjectId { get; set; }
    public string? ObjectType { get; set; }
}