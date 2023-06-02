using Domain.Entities;

namespace Application.Common.Interfaces;

public interface ICurrentUserService
{
    string GetRole();
    string? GetDepartment();
    User GetCurrentUser();
    Guid? GetCurrentRoomForStaff();
}