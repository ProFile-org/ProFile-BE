using Application.Common.Exceptions;
using Application.Folders.Commands;
using Domain.Entities;
using Domain.Entities.Physical;
using Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.Folders.Commands;

public class AddFolderTests : BaseClassFixture
{
    public AddFolderTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
    }

    [Fact]
    public async Task ShouldThrowLimitExceededException_WhenGoingOverCapacity()
    {
        // Arrange
        var department = CreateDepartment();
        var folder1 = CreateFolder();
        var folder2 = CreateFolder();
        var folder3 = CreateFolder();
        var locker = CreateLocker(folder1, folder2, folder3);
        var room = CreateRoom(department, locker);
        await AddAsync(room);

        var command = new AddFolder.Command()
        {
            Name = "something",
            Capacity = 3,
            LockerId = locker.Id,
            Description = "something else",
        };
        
        // Act
        var action = async () => await SendAsync(command);
        
        // Assert
        await action.Should().ThrowAsync<LimitExceededException>()
            .WithMessage("This locker cannot accept more folders.");
        
        // Cleanup
        Remove(folder1);
        Remove(folder2);
        Remove(folder3);
        Remove(locker);
        Remove(room);
        Remove(await FindAsync<Department>(department.Id));
    }

    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenLockerIdNotExists()
    {
        // Arrange
        var command =  new AddFolder.Command()
        {
            LockerId = Guid.NewGuid(),
            Name = "something",
            Capacity = 3
        };
        
        // Act
        var action = async () => await SendAsync(command);
        
        // Assert
        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Locker does not exist.");
    }
}