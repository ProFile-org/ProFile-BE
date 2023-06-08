using Application.Common.Exceptions;
using Application.UserGroups.Commands;
using Domain.Entities;
using Domain.Entities.Digital;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Application.Tests.Integration.UserGroups.Commands;

public class UpdateUserGroupTests : BaseClassFixture
{
    public UpdateUserGroupTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
        
    }

    [Fact]
    public async Task ShouldUpdateLocker_WhenDetailsAreValid()
    {
        // Arrange
        var userGroup = CreateUserGroup(Array.Empty<User>());
        await AddAsync(userGroup);

        var command = new UpdateUserGroup.Command()
        {
            UserGroupId = userGroup.Id,
            Name = "Updated name",
        };
        
        // Act
        var result = await SendAsync(command);
        
        // Assert
        result.Name.Should().Be(command.Name);
        
        // Cleanup
        Remove(await FindAsync<UserGroup>(userGroup.Id));
    }

    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenUserGroupDoesNotExist()
    {
        // Arrange
        var command = new UpdateUserGroup.Command()
        {
            UserGroupId = Guid.NewGuid(),
            Name = "Name",
        };
        
        // Act
        var result = async () => await SendAsync(command);
        
        // Assert
        await result.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("User group does not exist.");
    }

    [Fact]
    public async Task ShouldThrowConflictException_WhenNewUserGroupNameAlreadyExists()
    {
        // Arrange
        var userGroup = CreateUserGroup(Array.Empty<User>());
        await AddAsync(userGroup);
        
        var updateUserGroup = CreateUserGroup(Array.Empty<User>());
        await AddAsync(updateUserGroup);

        var command = new UpdateUserGroup.Command()
        {
            UserGroupId = updateUserGroup.Id,
            Name = userGroup.Name,
        };
        
        // Act
        var result = async () => await SendAsync(command);
        
        // Assert
        await result.Should().ThrowAsync<ConflictException>()
            .WithMessage("New user group name already exists.");
        
        // Cleanup
        Remove(userGroup);
        Remove(updateUserGroup);
    }
}