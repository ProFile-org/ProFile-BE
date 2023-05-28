﻿using Application.Common.Exceptions;
using Application.Lockers.Commands;
using Bogus;
using Domain.Entities.Physical;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.Lockers.Commands;

public class EnableLockerTests : BaseClassFixture
{
    public EnableLockerTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
        
    }

    [Fact]
    public async Task ShouldEnableLocker_WhenLockerExistsAndIsNotAvailable()
    {
        // Arrange
        var locker = CreateLocker();
        locker.IsAvailable = false;
        var room = CreateRoom(locker);
        await AddAsync(room);

        // Act
        var command = new EnableLocker.Command()
        {
            LockerId = locker.Id,
        };
        
        var result = await SendAsync(command);

        // Assert
        result.IsAvailable.Should().BeTrue();
        
        // Cleanup
        Remove(room);
    }
    
    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenLockerDoesNotExist()
    {
        // Arrange
        var command = new EnableLocker.Command()
        {
            LockerId = Guid.NewGuid(),
        };
        
        // Act
        var action = async () => await SendAsync(command);
        
        // Assert
        await action.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("Locker does not exist.");
    }

    [Fact]
    public async Task ShouldThrowConflictException_WhenLockerIsAlreadyEnabled()
    {
        // Arrange 
        var locker = CreateLocker();
        var room = CreateRoom(locker);
        await AddAsync(room);
        
        var enableLockerCommand = new EnableLocker.Command()
        {
            LockerId = locker.Id,
        };
        
        // Act 
        var action = async () => await SendAsync(enableLockerCommand);
        
        // Assert
        await action.Should()
            .ThrowAsync<ConflictException>()
            .WithMessage("Locker has already been enabled.");
        
        // Cleanup
        Remove(room);
    }
}
