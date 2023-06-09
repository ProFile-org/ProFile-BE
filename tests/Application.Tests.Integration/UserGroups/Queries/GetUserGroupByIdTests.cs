using Application.UserGroups.Queries;
using Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.UserGroups.Queries;

public class GetUserGroupByIdTests : BaseClassFixture
{
    public GetUserGroupByIdTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
    }

    [Fact]
    public async Task ShouldReturnUserGroup_WhenThatUserGroupExists()
    {
        // Arrange
        var userGroup = CreateUserGroup(Array.Empty<User>());

        await AddAsync(userGroup);

        var query = new GetUserGroupById.Query()
        {
            UserGroupId = userGroup.Id,
        };
        
        // Act
        var result = await SendAsync(query);
        
        // Assert
        result.Name.Should().Be(userGroup.Name);
        
        // Cleanup
        Remove(userGroup);
    }

    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenUsergroupDoesNotExist()
    {
        // Arrange
        var query = new GetUserGroupById.Query()
        {
            UserGroupId = Guid.NewGuid(),
        };
        
        // Act
        var action = async () => await SendAsync(query);
        
        // Assert
        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("User group does not exist.");
    }
}