using Application.Identity;
using Application.Users.Commands;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.Users.Commands;

public class UpdateUserTests : BaseClassFixture
{
    public UpdateUserTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
    }

    [Fact]
    public async Task ShouldUpdateUser_WhenUpdateDetailsAreValid()
    {
        // Arrange
        var user = CreateUser(IdentityData.Roles.Employee, "randompassword");
        await AddAsync(user);

        var command = new UpdateUser.Command()
        {
            UserId = user.Id,
            FirstName = "khoa",
            LastName = "ngu",
            Position = "IDK",
        };
        
        // Act
        var result = await SendAsync(command);
        
        // Assert
        result.FirstName.Should().Be(command.FirstName);
        result.LastName.Should().Be(command.LastName);
        result.Position.Should().Be(command.Position);
        
        // Cleanup
        Remove(user);
    }
    
    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenThatUserDoesNotExist()
    {
        // Arrange
        var command = new UpdateUser.Command()
        {
            UserId = Guid.NewGuid(),
            FirstName = "khoa",
            LastName = "ngu",
            Position = "IDK",
        };
        
        // Act
        var action = async () => await SendAsync(command);
        
        // Assert
        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("User does not exist.");
    }
}