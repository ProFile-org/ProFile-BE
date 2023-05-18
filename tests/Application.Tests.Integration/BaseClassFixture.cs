using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Application.Tests.Integration;

public class BaseClassFixture : IClassFixture<CustomApiFactory>
{
    private static IServiceScopeFactory _scopeFactory = null!;

    protected BaseClassFixture(CustomApiFactory apiFactory)
    {
        _scopeFactory = apiFactory.Services.GetRequiredService<IServiceScopeFactory>();
    }
    
    public static async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        using var scope = _scopeFactory.CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();

        return await mediator.Send(request);
    }
}