using Domain.Entities;

namespace Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid GetId();
    string GetRole();
    Guid GetDepartmentId();
    User GetCurrentUser();
    Guid? GetCurrentRoomForStaff();
    Guid? GetCurrentDepartmentForStaff();
}