using Application.Common.Interfaces;
using MediatR;

namespace Application.Common.Behaviors;

public class RequestAuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> 
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IAuthorizer<TRequest>> _authorizers;
    private readonly IMediator _mediator;


    public RequestAuthorizationBehavior(IEnumerable<IAuthorizer<TRequest>> authorizers, IMediator mediator)
    {
        _authorizers = authorizers;
        _mediator = mediator;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requirements = new List<IAuthorizationRequirement>();
        
        foreach (var authorizer in _authorizers)
        {
            authorizer.BuildPolicy(request);
            requirements.AddRange(authorizer.Requirements);
        }
 
        foreach (var requirement in requirements.Distinct())
        {
            var result = await _mediator.Send(requirement, cancellationToken);
            if (!result.IsAuthorized)
            {
                throw new UnauthorizedAccessException(result.FailureMessage);
            }
        }

        return await next();
    }
}
