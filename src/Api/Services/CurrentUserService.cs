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

    public string GetRole()
    {
        var userName = _httpContextAccessor.HttpContext!.User.Claims
            .FirstOrDefault(x => x.Type.Equals(JwtRegisteredClaimNames.Sub));
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

    public string? GetDepartment()
    {
        var userName = _httpContextAccessor.HttpContext!.User.Claims
            .FirstOrDefault(x => x.Type.Equals(JwtRegisteredClaimNames.Sub));
        if (userName is null)
        {
            throw new UnauthorizedAccessException();
        }

        var user = _context.Users.FirstOrDefault(x => x.Username.Equals(userName));

        if (user is null)
        {
            throw new UnauthorizedAccessException();
        }
        
        return user.Department?.Name;
    }

    public User GetCurrentUser()
    {
        var userName = _httpContextAccessor.HttpContext!.User.Claims
            .FirstOrDefault(x => x.Type.Equals(JwtRegisteredClaimNames.Sub));
        if (userName is null)
        {
            throw new UnauthorizedAccessException();
        }

        var user = _context.Users.FirstOrDefault(x => x.Username.Equals(userName));

        if (user is null)
        {
            throw new UnauthorizedAccessException();
        }
        
        return user;
    }

    public Guid? GetCurrentRoomForStaff()
    {
        var userName = _httpContextAccessor.HttpContext!.User.Claims
            .FirstOrDefault(x => x.Type.Equals(JwtRegisteredClaimNames.Sub));
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
}