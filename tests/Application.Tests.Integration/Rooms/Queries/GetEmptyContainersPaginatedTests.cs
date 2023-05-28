using Application.Rooms.Queries;
using Bogus;
using Domain.Entities.Physical;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Application.Tests.Integration.Rooms.Queries;

public class GetEmptyContainersPaginatedTests : BaseClassFixture
{
    public GetEmptyContainersPaginatedTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
    }

    [Fact]
    public async Task ShouldReturnLockersWithEmptyFolders()
    {
        // Arrange
        var department = CreateDepartment();
        var folder1 = CreateFolder();
        var folder2 = CreateFolder();
        var folder3 = CreateFolder();
        folder3.IsAvailable = false;
        var locker1 = CreateLocker(folder1, folder2);
        locker1.Capacity = 2;
        var locker2 = CreateLocker(folder3);
        locker2.Capacity = 2;
        var room = CreateRoom(department, locker1, locker2);
        await AddAsync(room);
        
        var query = new GetEmptyContainersPaginated.Query()
        {
            Page = 1,
            Size = 2,
            RoomId = room.Id
        };

        // Act
        var result = await SendAsync(query);

        // Assert
        result.TotalCount.Should().Be(1);
        result.Items.First().Id.Should().Be(room.Lockers.ElementAt(0).Id);
        result.Items.First().Name.Should().Be(room.Lockers.ElementAt(0).Name);
        result.Items.First().Description.Should().Be(room.Lockers.ElementAt(0).Description);
        result.Items.First().NumberOfFreeFolders.Should().Be(2);
        result.Items.First().Capacity.Should().Be(2);

        // Cleanup
        Remove(folder1);
        Remove(folder2);
        Remove(folder3);
        Remove(locker1);
        Remove(locker2);
        Remove(room);
        Remove(department);
    }

    [Fact]
    public async Task ShouldThrowNotFound_WhenRoomDoesNotExist()
    {
        // Arrange
        var query = new GetEmptyContainersPaginated.Query()
        {
            Page = 1,
            Size = 2,
            RoomId = Guid.NewGuid()
        };

        // Act
        var action = async () => await SendAsync(query);

        // Assert
        await action.Should().ThrowAsync<KeyNotFoundException>("Room does not exist");
    }
}