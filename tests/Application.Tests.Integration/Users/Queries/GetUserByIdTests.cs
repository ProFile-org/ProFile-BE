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