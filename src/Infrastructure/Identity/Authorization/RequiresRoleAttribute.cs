using System.IdentityModel.Tokens.Jwt;
using Application.Identity;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Identity.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequiresRoleAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] _claimValues;

    public RequiresRoleAttribute(params string[] claimValues)
    {
        _claimValues = claimValues;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var dbContext = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();

        var email = context.HttpContext.User.Claims.SingleOrDefault(y => y.Type.Equals(JwtRegisteredClaimNames.Email))!.Value;

        var user = dbContext.Users.FirstOrDefault(x => x.Email!.Equals(email));

        foreach (var claimValue in _claimValues)
        {
            if (user!.Role.Equals(claimValue))
            {
                return;
            }
        }

        context.Result = new ForbidResult();
    }
}