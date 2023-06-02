using Application.Common.Exceptions;
using Application.Identity;
using Application.Staffs.Commands;
using Domain.Entities;
using Domain.Entities.Physical;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.Staffs.Commands;

public class RemoveStaffFromRoomTests : BaseClassFixture
{
    public RemoveStaffFromRoomTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
    }

    [Fact]
    public async Task ShouldUnasignStaff_WhenStaffHaveARoom()
    {
        // Arrange
        var department = CreateDepartment();
        var room = CreateRoom(department);
        var user = CreateUser(IdentityData.Roles.Admin, "123123");
        var staff = CreateStaff(user, room);
        await AddAsync(staff);

        var command = new RemoveStaffFromRoom.Command()
        {
            StaffId = staff.Id
        };

        // Act
        var result = await SendAsync(command);

        // Assert
        var assertionRoom = await FindAsync<Room>(room.Id);
        result.Room.Should().BeNull();
        assertionRoom.Staff.Should().BeNull();
        
        // Cleanup
        Remove(staff);
        Remove(await FindAsync<User>(user.Id));
        Remove(await FindAsync<Room>(room.Id));
        Remove(await FindAsync<Department>(department.Id));
    }

    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenStaffDoesNotExist()
    {
        // Arrange
        var command = new RemoveStaffFromRoom.Command()
        {
            StaffId = Guid.NewGuid()
        };

        // Act
        var action = async () => await SendAsync(command);

        // Assert
        await action.Should().ThrowAsync<KeyNotFoundException>("Staff does not exist.");
    }

    [Fact]
    public async Task ShouldThrowConflictException_WhenStaffIsNotAssignedToARoom()
    {
        // Arrange
        var user = CreateUser(IdentityData.Roles.Admin,"123123");
        var staff = CreateStaff(user, null);
        await AddAsync(staff);
        
        var command = new RemoveStaffFromRoom.Command()
        {
            StaffId = staff.Id
        };
        
        // Act
        var action = async () => await SendAsync(command);

        // Assert
        await action.Should().ThrowAsync<ConflictException>()
            .WithMessage("Staff is not assigned to a room.");
        
        // Cleanup
        Remove(staff);
        Remove(await FindAsync<User>(user.Id));
    }
}