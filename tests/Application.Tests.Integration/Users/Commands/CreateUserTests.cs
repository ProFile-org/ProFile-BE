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
    public CreateUserTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
    }

    [Fact]
    public async Task ShouldCreateUser_WhenCreateDetailsAreValid()
    {
        // Arrange
        var department = new Department()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Commerce.Department()
        };
        await AddAsync(department);
        var createUserCommand = _userGenerator.Generate();
        createUserCommand = createUserCommand with
        {
            DepartmentId = department.Id
        };
        
        // Act
        var user = await SendAsync(createUserCommand);
        
        // Assert
        user.Username.Should().Be(createUserCommand.Username);
        user.FirstName.Should().Be(createUserCommand.FirstName);
        user.LastName.Should().Be(createUserCommand.LastName);
        user.Department.Should().BeEquivalentTo(new { department.Id, department.Name });
        user.Email.Should().Be(createUserCommand.Email);
        user.Role.Should().Be(createUserCommand.Role);
        user.Position.Should().Be(createUserCommand.Position);
        user.IsActive.Should().Be(true);
        user.IsActivated.Should().Be(false);
        
        // Clean up
        var userEntity = await FindAsync<User>(user.Id);
        Remove(userEntity);
        Remove(department);
    }
}