using Application.Common.Models;
using MediatR;

namespace Application.Common.Interfaces;

public interface IAuthorizationRequirement : IRequest<AuthorizationResult>
{
    
}