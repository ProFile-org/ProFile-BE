using Application.Rooms.Queries;
using Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.Rooms.Queries;

public class GetRoomByIdTests : BaseClassFixture
{
    public GetRoomByIdTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
    }
    
    [Fact]
    public async Task ShouldReturnRoom_WhenThatRoomExists()
    {
        // Arrange
        var department = CreateDepartment();
        var room = CreateRoom(department);
        await AddAsync(room);
        
        var query = new GetRoomById.Query()
        {
            RoomId = room.Id,
        };

        // Act
        var result = await SendAsync(query);

        // Assert
        result.Id.Should().Be(room.Id);
        result.Name.Should().Be(room.Name);

        // Cleanup
        Remove(room);
        Remove(await FindAsync<Department>(department.Id));
    }
    
    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenThatRoomDoesNotExist()
    {
        // Arrange
        var query = new GetRoomById.Query()
        {
            RoomId = Guid.NewGuid(),
        };

        // Act
        var action = async () => await SendAsync(query);

        // Assert
        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Room does not exist.");
    }
}