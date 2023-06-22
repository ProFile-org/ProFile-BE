using Application.Common.Exceptions;
using Application.UserGroups.Commands;
using Bogus;
using Domain.Entities;
using Domain.Entities.Digital;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.UserGroups.Commands;

public class CreateUserGroupTests : BaseClassFixture
{
    public CreateUserGroupTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
        
    }

    [Fact]
    public async Task ShouldCreateUserGroup_WhenCreateDetailsAreValid()
    {
        // Arrange
        var command = new CreateUserGroup.Command()
        {
            Name = new Faker().Commerce.Department(),
        };
        
        // Act
        var result = await SendAsync(command);
        
        // Assert
        result.Name.Should().Be(command.Name);
        
        // Cleanup
        Remove(await FindAsync<UserGroup>(result.Id));
    }
    
    [Fact]
    public async Task ShouldThrowConflictException_WhenUserGroupNameExists()
    {
        // Arrange
        var userGroup = CreateUserGroup(new User[] {});
        await AddAsync(userGroup);
        
        var command = new CreateUserGroup.Command()
        {
            Name = userGroup.Name,
        };
        
        // Act
        var action = async () => await SendAsync(command);
        
        // Assert
        await action.Should().ThrowAsync<ConflictException>().WithMessage("User group name already exists.");
        
        // Cleanup
        Remove(userGroup);
    }
}