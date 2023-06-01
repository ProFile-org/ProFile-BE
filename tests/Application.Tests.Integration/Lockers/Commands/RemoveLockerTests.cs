using Application.Lockers.Commands;
using Application.Rooms.Commands;
using Domain.Entities;
using Domain.Entities.Physical;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.Lockers.Commands;

public class RemoveLockerTests : BaseClassFixture
{
    public RemoveLockerTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
    }

    [Fact]
    public async Task ShouldRemoveLocker_WhenLockerHasNoDocuments()
    {
        // Arrange
        var department = CreateDepartment();
        var locker = CreateLocker();
        var room = CreateRoom(department, locker);
        await Add(room);

        var command = new RemoveLocker.Command()
        {
            LockerId = locker.Id,
        };
        
        // Act
        var result = await SendAsync(command);

        // Assert
        result.Id.Should().Be(locker.Id);
        var removedLocker = await FindAsync<Locker>(locker.Id);
        removedLocker.Should().BeNull();
        
        // Cleanup
        Remove(await FindAsync<Room>(room.Id));
        Remove(await FindAsync<Department>(department.Id));
    }

    [Fact]
    public async Task ShouldThrowConflictException_WhenRoomStillHasDocuments()
    {
        // Arrange
        var department = CreateDepartment();
        var documents = CreateNDocuments(1);
        var folder = CreateFolder(documents);
        var locker = CreateLocker(folder);
        var room = CreateRoom(department, locker);
        await AddAsync(room);

        var command = new RemoveLocker.Command()
        {
            LockerId = locker.Id,
        };
        
        // Act
        var action = async () => await SendAsync(command);
        
        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Locker cannot be removed because it contains documents.");
        
        // Cleanup
        Remove(documents.First());
        Remove(folder);
        Remove(locker);
        Remove(room);
        Remove(await FindAsync<Department>(department.Id));
    }

    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenLockerDoesNotExist()
    {
        // Arrange
        var command = new RemoveLocker.Command()
        {
            LockerId = Guid.NewGuid(),
        };
        
        // Act
        var action = async () => await SendAsync(command);
        
        // Assert
        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Locker does not exist.");
    }
}