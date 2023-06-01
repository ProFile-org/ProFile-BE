using Application.Identity;
using Application.Users.Queries;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.Users.Queries;

public class GetUserByIdTests : BaseClassFixture
{
    public GetUserByIdTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
    }

    [Fact]
    public async Task ShouldReturnUser_WhenThatUserExists()
    {
        // Arrange
        var user = CreateUser(IdentityData.Roles.Employee, "randomPassword");
        await AddAsync(user);

        var query = new GetUserById.Query()
        {
            UserId = user.Id,
        };
        
        // Act
        var result = await SendAsync(query);
        
        // Assert
        result.Username.Should().Be(user.Username);
        result.Email.Should().Be(user.Email);
        result.FirstName.Should().Be(user.FirstName);
        result.LastName.Should().Be(user.LastName);
        result.Role.Should().Be(user.Role);
        result.Position.Should().Be(user.Position);
        result.IsActivated.Should().Be(user.IsActivated);
        result.IsActive.Should().Be(user.IsActive);
        
        // Cleanup
        Remove(user);
    }
    
    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenThatUserDoesNotExist()
    {
        // Arrange
        var query = new GetUserById.Query()
        {
            UserId = Guid.NewGuid(),
        };
        
        // Act
        var action = async () => await SendAsync(query);
        
        // Assert
        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("User does not exist.");
    }
}