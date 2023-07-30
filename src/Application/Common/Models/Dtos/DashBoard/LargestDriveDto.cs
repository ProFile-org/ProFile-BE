using Application.Users.Queries;

namespace Application.Common.Models.Dtos.DashBoard;

public class LargestDriveDto
{
    public string Label { get; set; }
    public long Value { get; set; }
    public UserDto User { get; set; }
}