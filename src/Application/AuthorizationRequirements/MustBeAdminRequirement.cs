using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.AuthorizationRequirements;

public class MustBeAdminRequirement : IAuthorizationRequirement
{
    public string Role { get; set; }
}

public class MustBeAdminRequirementHandler : IAuthorizationHandler<MustBeAdminRequirement>
{
    private readonly IApplicationDbContext _context;

    public MustBeAdminRequirementHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AuthorizationResult> Handle(MustBeAdminRequirement request, CancellationToken cancellationToken)
    {
        if (!request.Role.Equals(IdentityData.Roles.Admin))
        {
            return AuthorizationResult.Fail("You are not admin");
        }

        return AuthorizationResult.Succeed();
    }
}