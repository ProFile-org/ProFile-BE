using Application.Users.Queries;
using NodaTime;

namespace Application.Common.Models.Dtos;

public class LogDto {
    public int Id { get; set; }
    public string? Template { get; set; }
    public string? Message { get; set; }
    public string? Level { get; set; }
    public Instant? Time { get; set; }
    public string? Event { get; set; }
    public UserDto? User { get; set; }
    public BaseDto? Object { get; set; }
    public string? ObjectType { get; set; }
}