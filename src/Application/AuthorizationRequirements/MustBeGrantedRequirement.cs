using Application.Common.AccessControl.Models;
using Application.Common.AccessControl.Models.Operations;
using Application.Common.Interfaces;
using Application.Common.Models;

namespace Application.AuthorizationRequirements;

public class MustBeGrantedRequirement : IAuthorizationRequirement
{
    public PhysicalResource Resource { get; set; }
    public Enum Operation { get; set; }
    
}

public class MustBeGrantedRequirementHandler : IAuthorizationHandler<MustBeGrantedRequirement>
{
    private readonly IPhysicalAccessControlList _physicalAccessControlList;
    private readonly ICurrentUserService _currentUserService;

    public MustBeGrantedRequirementHandler(IPhysicalAccessControlList physicalAccessControlList, ICurrentUserService currentUserService)
    {
        _physicalAccessControlList = physicalAccessControlList;
        _currentUserService = currentUserService;
    }

    public async Task<AuthorizationResult> Handle(MustBeGrantedRequirement request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetId();
        
        var principal = new PhysicalPrincipal()
        {
            UserId = userId,
        };
        
        if (!_physicalAccessControlList.IsGranted(request.Resource, request.Operation, principal))
        {
            return AuthorizationResult.Fail("You are not permitted to do this.");
        }
        
        return AuthorizationResult.Succeed();
    }
}