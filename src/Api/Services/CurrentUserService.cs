using System.IdentityModel.Tokens.Jwt;
using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor, IApplicationDbContext context)
    {
        _httpContextAccessor = httpContextAccessor;
        _dbContext = context;
    }

    public Guid GetId()
    {
        var id =  _httpContextAccessor.HttpContext!.User.Claims
            .FirstOrDefault(x => x.Type.Equals(JwtRegisteredClaimNames.NameId))!.Value;
        return Guid.Parse(id);
    }

    public string GetRole()
    {
        var userName = _httpContextAccessor.HttpContext!.User.Claims
            .FirstOrDefault(x => x.Type.Equals(JwtRegisteredClaimNames.Sub))!.Value;
        if (userName is null)
        {
            throw new UnauthorizedAccessException();
        }

        var user = _dbContext.Users.FirstOrDefault(x => x.Username.Equals(userName));

        if (user is null)
        {
            throw new UnauthorizedAccessException();
        }
        
        return user.Role;
    }

    public Guid GetDepartmentId()
    {
        var claim =  _httpContextAccessor.HttpContext!.User.Claims
            .FirstOrDefault(x => x.Type.Equals("departmentId"));
        var id = claim?.Value;
        return Guid.Parse(id!);
    }

    public User GetCurrentUser()
    {
        var userName = _httpContextAccessor.HttpContext!.User.Claims
            .FirstOrDefault(x => x.Type.Equals(JwtRegisteredClaimNames.Sub))!.Value;
        if (userName is null)
        {
            throw new UnauthorizedAccessException();
        }

        var user = _dbContext.Users
            .Include(x => x.Department)
            .FirstOrDefault(x => x.Username.Equals(userName));

        if (user is null)
        {
            throw new UnauthorizedAccessException();
        }
        
        return user;
    }

    public Guid? GetCurrentRoomForStaff()
    {
        var userIdString = _httpContextAccessor.HttpContext!.User.Claims
            .FirstOrDefault(x => x.Type.Equals(JwtRegisteredClaimNames.NameId));
        if (userIdString is null || !Guid.TryParse(userIdString.Value, out var userId))
        {
            throw new UnauthorizedAccessException();
        }

        var staff = _dbContext.Staffs
            .Include(x => x.User)
            .Include(x => x.Room)
            .FirstOrDefault(x => x.Id == userId);
        
        return staff?.Room?.Id;
    }
    
    public Guid? GetCurrentDepartmentForStaff()
    {
        var userIdString = _httpContextAccessor.HttpContext!.User.Claims
            .FirstOrDefault(x => x.Type.Equals(JwtRegisteredClaimNames.NameId));
        if (userIdString is null || !Guid.TryParse(userIdString.Value, out var userId))
        {
            throw new UnauthorizedAccessException();
        }

        var staff = _dbContext.Staffs
            .Include(x => x.User)
            .Include(x => x.Room)
            .FirstOrDefault(x => x.Id == userId);
        
        return staff?.Room?.DepartmentId;
    }
}