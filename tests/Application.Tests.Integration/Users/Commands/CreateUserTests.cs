using Application.Departments.Commands.CreateDepartment;
using Application.Users.Commands.CreateUser;
using Application.Users.Queries;
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
        var createDepartmentCommand = _departmentGenerator.Generate();
        var department = await SendAsync(createDepartmentCommand);
        var command = _userGenerator.Generate();
        command = command with
        {
            DepartmentId = department.Id
        };
        
        // Act
        var result = await SendAsync(command);
        
        // Assert
        result.Username.Should().Be(command.Username);
        result.FirstName.Should().Be(command.FirstName);
        result.LastName.Should().Be(command.LastName);
        result.Department.Should().BeEquivalentTo(department);
        result.Email.Should().Be(command.Email);
        result.Role.Should().Be(command.Role);
        result.Position.Should().Be(command.Position);
        result.IsActive.Should().Be(true);
        result.IsActivated.Should().Be(false);
        
    }
}