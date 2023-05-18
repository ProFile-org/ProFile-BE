using Application.Users.Commands.CreateUser;
using Bogus;
using Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.Users.Commands;

public class CreateUserTests : BaseClassFixture
{
    private readonly Faker<CreateUserCommand> _userGenerator = new Faker<CreateUserCommand>()
        .RuleFor(x => x.Username, faker => faker.Person.UserName)
        .RuleFor(x => x.Email, faker => faker.Person.Email)
        .RuleFor(x => x.FirstName, faker => faker.Person.FirstName)
        .RuleFor(x => x.LastName, faker => faker.Person.LastName)
        .RuleFor(x => x.Password, faker => faker.Random.String())
        .RuleFor(x => x.Role, faker => faker.Random.Word())
        .RuleFor(x => x.Position, faker => faker.Random.Word());
    protected CreateUserTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
    }

    [Fact]
    public async Task ShouldCreateUser_WhenCreateDetailsAreValid()
    {
        // Arrange
        var command = _userGenerator.Generate();
        
        // Act
        var result = await SendAsync(command);
        
        // Assert
        result.Should().BeEquivalentTo(command);
    }
}