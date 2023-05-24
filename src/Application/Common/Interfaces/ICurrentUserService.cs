namespace Application.Common.Interfaces;

public interface ICurrentUserService
{
    string GetRole();
    string? GetDepartment();
}