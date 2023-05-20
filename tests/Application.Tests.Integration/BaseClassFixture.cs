using Application.Departments.Commands.CreateDepartment;
using Bogus;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Application.Tests.Integration;

public class BaseClassFixture : IClassFixture<CustomApiFactory>
{
    protected readonly Faker<CreateDepartmentCommand> _departmentGenerator = new Faker<CreateDepartmentCommand>()
        .RuleFor(x => x.Name, faker => faker.Commerce.Department());
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