using System.ComponentModel.DataAnnotations;
using NodaTime;

namespace Domain.Entities;

public class RefreshToken
{
    [Key]
    public Guid Token { get; set; }
    public string JwtId { get; set; } = null!;
    public LocalDateTime CreationDateTime { get; set; }
    public LocalDateTime ExpiryDateTime { get; set; }
    public bool IsUsed { get; set; }
    public bool IsInvalidated { get; set; }
    public User User { get; set; } = null!;
}