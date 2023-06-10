using Application.Common.Models;
using MediatR;

namespace Application.Common.Interfaces;

public interface IAuthorizationHandler<TRequest> : IRequestHandler<TRequest, AuthorizationResult> 
    where TRequest : IRequest<AuthorizationResult>{}