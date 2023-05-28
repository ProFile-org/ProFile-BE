using Application.Users.Commands;
using Bogus;
using Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.Users.Commands;

public class AddUserTests : BaseClassFixture
{
    public AddUserTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
    }

    [Fact]
    public async Task ShouldCreateUser_WhenCreateDetailsAreValid()
    {
        // Arrange
        var department = CreateDepartment();
        await AddAsync(department);
        
        var command = new AddUser.Command()
        {
            Username = new Faker().Person.UserName,
            Email = new Faker().Person.Email,
            FirstName = new Faker().Person.FirstName,
            LastName = new Faker().Person.LastName,
            Password = new Faker().Random.Word(),
            Role = new Faker().Random.Word(),
            DepartmentId = department.Id,
            Position = new Faker().Random.Word(),
        };
        
        // Act
        var user = await SendAsync(command);
        
        // Assert
        user.Username.Should().Be(command.Username);
        user.FirstName.Should().Be(command.FirstName);
        user.LastName.Should().Be(command.LastName);
        user.Department.Should().BeEquivalentTo(new { department.Id, department.Name });
        user.Email.Should().Be(command.Email);
        user.Role.Should().Be(command.Role);
        user.Position.Should().Be(command.Position);
        user.IsActive.Should().Be(true);
        user.IsActivated.Should().Be(false);
        
        // Clean up
        var userEntity = await FindAsync<User>(user.Id);
        Remove(userEntity);
        Remove(department);
    }
}