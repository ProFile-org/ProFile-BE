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