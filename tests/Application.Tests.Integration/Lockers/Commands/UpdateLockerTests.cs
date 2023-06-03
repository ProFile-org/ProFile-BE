using Application.Common.Exceptions;
using Application.Lockers.Commands;
using Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.Lockers.Commands;

public class UpdateLockerTests : BaseClassFixture
{
    public UpdateLockerTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
    }

    [Fact]
    public async Task ShouldUpdateLocker_WhenUpdateDetailsAreValid()
    {
        // Arrange
        var department = CreateDepartment();
        var locker = CreateLocker();
        var room = CreateRoom(department, locker);
        await AddAsync(room);

        var command = new UpdateLocker.Command()
        {
            LockerId = locker.Id,
            Name = "Something else",
            Capacity = 6,
            Description = "ehehe",
        };
        
        // Act
        var result = await SendAsync(command);
        
        // Assert
        result.Id.Should().Be(locker.Id);
        result.Name.Should().Be(command.Name);
        result.Capacity.Should().Be(command.Capacity);
        result.Description.Should().Be(command.Description);
        
        // Cleanup
        Remove(locker);
        Remove(room);
        Remove(await FindAsync<Department>(department.Id));
    }
    
    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenThatLockerDoesNotExist()
    {
        // Arrange
        var command = new UpdateLocker.Command()
        {
            LockerId = Guid.NewGuid(),
            Name = "Something else",
            Capacity = 6,
            Description = "ehehe",
        };
        
        // Act
        var result = async () => await SendAsync(command);
        
        // Assert
        await result.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Locker does not exist.");
    }
    
    [Fact]
    public async Task ShouldThrowConflictException_WhenNewLockerNameHasAlreadyExistedInThatRoom()
    {
        // Arrange
        var department = CreateDepartment();
        var duplicateNameLocker = CreateLocker();
        var locker = CreateLocker();
        var room = CreateRoom(department, duplicateNameLocker, locker);
        await AddAsync(room);
        
        var command = new UpdateLocker.Command()
        {
            LockerId = locker.Id,
            Name = duplicateNameLocker.Name,
            Capacity = 6,
            Description = "ehehe",
        };
        
        // Act
        var result = async () => await SendAsync(command);
        
        // Assert
        await result.Should().ThrowAsync<ConflictException>()
            .WithMessage("New locker name already exists.");
        
        // Cleanup
        Remove(locker);
        Remove(room);
        Remove(await FindAsync<Department>(department.Id));
    }
    
    [Fact]
    public async Task ShouldThrowConflictException_WhenNewCapacityIsLessThanCurrentNumberOfFolders()
    {
        // Arrange
        var department = CreateDepartment();
        var folder1 = CreateFolder();
        var folder2 = CreateFolder();
        var locker = CreateLocker(folder1, folder2);
        locker.Capacity = 3;
        var room = CreateRoom(department, locker);
        await AddAsync(room);
        
        var command = new UpdateLocker.Command()
        {
            LockerId = locker.Id,
            Name = "Something else",
            Capacity = 1,
            Description = "ehehe",
        };
        
        // Act
        var result = async () => await SendAsync(command);
        
        // Assert
        await result.Should().ThrowAsync<ConflictException>()
            .WithMessage("New capacity cannot be less than current number of folders.");
        
        // Cleanup
        Remove(folder1);
        Remove(folder2);
        Remove(locker);
        Remove(room);
        Remove(await FindAsync<Department>(department.Id));
    }

    [Fact]
    public async Task ShouldUpdateLocker_WhenSameNameExistsButInDifferentRoom()
    {
        // Arrange
        var department1 = CreateDepartment();
        var department2 = CreateDepartment();
        var locker = CreateLocker();
        var duplicateNameLocker = CreateLocker();
        var room1 = CreateRoom(department1, locker);
        var room2 = CreateRoom(department2, duplicateNameLocker);
        await AddAsync(room1);
        await AddAsync(room2);

        var command = new UpdateLocker.Command()
        {
            LockerId = locker.Id,
            Name = duplicateNameLocker.Name,
            Capacity = 23,
            Description = "fuck",
        };
        
        // Act
        var result = await SendAsync(command);
        
        // Assert
        result.Id.Should().Be(locker.Id);
        result.Name.Should().Be(command.Name);
        result.Capacity.Should().Be(command.Capacity);
        result.Description.Should().Be(command.Description);
        
        // Cleanup
        Remove(locker);
        Remove(duplicateNameLocker);
        Remove(room1);
        Remove(room2);
        Remove(await FindAsync<Department>(department1.Id));
        Remove(await FindAsync<Department>(department2.Id));
    }
}