using System.ComponentModel.DataAnnotations;
using NodaTime;

namespace Domain.Entities;

public class ResetPasswordToken
{
    [Key] public string TokenHash { get; set; } = null!;
    public User User { get; set; } = null!;
    public LocalDateTime ExpirationDate { get; set; }
    public bool IsInvalidated { get; set; }
}