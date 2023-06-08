using Application.UserGroups.Commands;
using Domain.Entities;
using Domain.Entities.Digital;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.UserGroups.Commands;

public class DeleteUserGroupTests : BaseClassFixture
{
    public DeleteUserGroupTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
        
    }

    [Fact]
    public async Task ShouldDeleteUserGroup_WhenUserGroupExists()
    {
        // Arrange
        var userGroup = CreateUserGroup(Array.Empty<User>());
        await AddAsync(userGroup);

        var command = new DeleteUserGroup.Command()
        {
            UserGroupId = userGroup.Id,
        };
        
        // Act
        var result = await SendAsync(command);
        
        // Assert
        var entityCheck = await FindAsync<UserGroup>(result.Id);
        entityCheck.Should().BeNull();
    }

    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenUserGroupDoesNotExist()
    {
        // Arrange
        var command = new DeleteUserGroup.Command()
        {
            UserGroupId = Guid.NewGuid(),
        };
        
        // Act
        var result = async () => await SendAsync(command);
        
        // Assert
        await result.Should().ThrowAsync<KeyNotFoundException>("User group does not exist.");
    }
}