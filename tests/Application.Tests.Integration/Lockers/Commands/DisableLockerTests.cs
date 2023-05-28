﻿using Application.Common.Exceptions;
using Application.Lockers.Commands;
using Domain.Entities;
using Domain.Entities.Physical;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.Lockers.Commands;

public class DisableLockerTests : BaseClassFixture
{
    public DisableLockerTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
    }

    [Fact]
    public async Task ShouldDisableLocker_WhenLockerExistsAndIsAvailable()
    {
        // Arrange
        var department = CreateDepartment();
        var locker = CreateLocker();
        var room = CreateRoom(department, locker);
        await AddAsync(room);

        var disableLockerCommand = new DisableLocker.Command()
        {
            LockerId = locker.Id,
        };
        
        // Act
        var result = await SendAsync(disableLockerCommand);
        
        // Assert
        result.Name.Should().Be(locker.Name);
        result.Description.Should().Be(locker.Description);
        result.Capacity.Should().Be(locker.Capacity);
        result.IsAvailable.Should().BeFalse();
        result.NumberOfFolders.Should().Be(locker.NumberOfFolders);
        
        // Cleanup
        Remove(room);
        Remove(await FindAsync<Department>(department.Id));
    }
    
    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenLockerDoesNotExist()
    {
        // Arrange
        var disableLockerCommand = new DisableLocker.Command()
        {
            LockerId = Guid.NewGuid(),
        };
        
        // Act
        var action = async () => await SendAsync(disableLockerCommand);
        
        // Assert
        await action.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("Locker does not exist.");
    }

    [Fact]
    public async Task ShouldThrowConflictException_WhenLockerIsAlreadyDisabled()
    {
        // Arrange 
        var department = CreateDepartment();
        var locker = CreateLocker();
        var room = CreateRoom(department, locker);
        await AddAsync(room);
        
        var disableLockerCommand = new DisableLocker.Command()
        {
            LockerId = locker.Id,
        };
        
        // Act 
        await SendAsync(disableLockerCommand);
        var action = async () => await SendAsync(disableLockerCommand);
        
        // Assert
        await action.Should().ThrowAsync<ConflictException>().WithMessage("Locker has already been disabled.");
        
        // Cleanup
        Remove(room);
        Remove(await FindAsync<Department>(department.Id));
    }
}