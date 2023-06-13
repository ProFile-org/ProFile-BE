using System.IdentityModel.Tokens.Jwt;
using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor, IApplicationDbContext context)
    {
        _httpContextAccessor = httpContextAccessor;
        _context = context;
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

        var user = _context.Users.FirstOrDefault(x => x.Username.Equals(userName));

        if (user is null)
        {
            throw new UnauthorizedAccessException();
        }
        
        return user.Role;
    }

    public Guid? GetDepartmentId()
    {
        var claim =  _httpContextAccessor.HttpContext!.User.Claims
            .FirstOrDefault(x => x.Type.Equals("departmentId"));
        var id = claim?.Value;
        return id is not null && Guid.TryParse(id, out _) ? Guid.Parse(id) : null;
    }

    public User GetCurrentUser()
    {
        var userName = _httpContextAccessor.HttpContext!.User.Claims
            .FirstOrDefault(x => x.Type.Equals(JwtRegisteredClaimNames.Sub))!.Value;
        if (userName is null)
        {
            throw new UnauthorizedAccessException();
        }

        var user = _context.Users
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
        var userName = _httpContextAccessor.HttpContext!.User.Claims
            .FirstOrDefault(x => x.Type.Equals(JwtRegisteredClaimNames.Sub))!.Value;
        if (userName is null)
        {
            throw new UnauthorizedAccessException();
        }

        var staff = _context.Staffs
            .Include(x => x.User)
            .Include(x => x.Room)
            .FirstOrDefault(x => x.User.Username.Equals(userName));

        if (staff is null)
        {
            throw new UnauthorizedAccessException();
        }
        
        return staff.Room!.Id;
    }
    
    public Guid? GetCurrentDepartmentForStaff()
    {
        var userName = _httpContextAccessor.HttpContext!.User.Claims
            .FirstOrDefault(x => x.Type.Equals(JwtRegisteredClaimNames.Sub))!.Value;
        if (userName is null)
        {
            throw new UnauthorizedAccessException();
        }

        var staff = _context.Staffs
            .Include(x => x.User)
            .Include(x => x.Room)
            .FirstOrDefault(x => x.User.Username.Equals(userName));

        if (staff is null)
        {
            throw new UnauthorizedAccessException();
        }

        if (staff.Room is null)
        {
            throw new UnauthorizedAccessException();
        }
        
        return staff.Room!.DepartmentId;
    }
}