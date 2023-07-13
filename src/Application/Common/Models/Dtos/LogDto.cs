using Application.Users.Queries;
using NodaTime;

namespace Application.Common.Models.Dtos;

public class LogDto {
    public int Id { get; set; }
    public string? Template { get; set; }
    public string? Message { get; set; }
    public string? Level { get; set; }
    public DateTime? Time { get; set; }
    public string? Event { get; set; }
    public UserDto? User { get; set; }
    public Guid? ObjectId { get; set; }
    public string? ObjectType { get; set; }
}