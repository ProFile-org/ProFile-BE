using System.Collections.Immutable;
using System.IdentityModel.Tokens.Jwt;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Identity.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequiresRoleAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] _claimValues;
    public static Dictionary<Guid, DateTime> OnlineUsers { get; } = new();

    public RequiresRoleAttribute(params string[] claimValues)
    {
        _claimValues = claimValues;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var dbContext = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();

        var email = context.HttpContext.User.Claims.SingleOrDefault(y => y.Type.Equals(JwtRegisteredClaimNames.Email))!.Value;

        var user = dbContext.Users.FirstOrDefault(x => x.Email!.Equals(email));

        if (user is null)
        {
            context.Result = new ForbidResult();
            return;
        }
        
        if (_claimValues.Any(claimValue => user!.Role.Equals(claimValue)))
        {
            OnlineUsers[user.Id] = DateTime.Now;
            return;
        }

        context.Result = new ForbidResult();
    }
}