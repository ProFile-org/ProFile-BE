using Application.Identity;
using Application.Staffs.Commands;
using Domain.Entities;
using Domain.Entities.Physical;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.Staffs.Commands;

public class RemoveStaffTests : BaseClassFixture
{
    public RemoveStaffTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
    }

    [Fact]
    public async Task ShouldRemoveStaff_WhenStaffIdIsValid()
    {
        // Arrange
        var department = CreateDepartment();
        var user = CreateUser(IdentityData.Roles.Admin, "123456");
        var room = CreateRoom(department);
        var staff = CreateStaff(user, room);
        await AddAsync(staff);

        var command = new RemoveStaff.Command()
        {
            StaffId = staff.Id
        };
        
        // Act
        await SendAsync(command);

        // Assert
        var result = await FindAsync<Staff>(staff.Id);
        result.Should().BeNull();

        // Cleanup
        Remove(await FindAsync<Room>(room.Id));
        Remove(user);
        Remove(await FindAsync<Department>(department.Id));
    }

    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenStaffDoesNotExist()
    {
        // Arrange
        var command = new RemoveStaff.Command()
        {
            StaffId = Guid.NewGuid()
        };

        // Act 
        var action = async () => await SendAsync(command);

        // Assert
        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Staff does not exist.");
    }
}