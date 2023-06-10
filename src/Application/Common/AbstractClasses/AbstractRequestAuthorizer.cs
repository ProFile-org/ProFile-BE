using Application.Common.Interfaces;

namespace Application.Common.AbstractClasses;

public abstract class AbstractRequestAuthorizer<TRequest> : IAuthorizer<TRequest>
{
    private HashSet<IAuthorizationRequirement> _requirements { get; set; } = new HashSet<IAuthorizationRequirement>();

    public IEnumerable<IAuthorizationRequirement> Requirements => _requirements;

    protected void UseRequirement(IAuthorizationRequirement requirement)
    {
        if (requirement is null) return;
        _requirements.Add(requirement);
    }

    public abstract void BuildPolicy(TRequest request);
}